#include <iostream>
#include <random>
#include <iomanip>
#include <cstring>
#include "hmm.hpp"

using namespace std;

const int THREADS_PER_BLOCK = 1024;


__global__
void make_forward_step(double *alpha, double *transitions, double *emissions, int *observations,
                       int t, int n_states, int observations_dict_size) {
    int i = threadIdx.x + blockIdx.x * blockDim.x;
    if (i < n_states) {
        alpha[t * n_states + i] = 0.0;
        for (int j = 0; j < n_states; ++j) {
            alpha[t * n_states + i] += alpha[(t - 1) * n_states + j] 
                                        * transitions[j * n_states + i] 
                                        * emissions[i * observations_dict_size + observations[t]];
        }
    }
}


template <typename T>
T *allocate_matrix_on_device(int rows, int cols){
    T *matrix;
    cudaMalloc(&matrix, sizeof(T) * rows * cols);
    return matrix;
}


template <typename T>
void copy_matrix_to_device(T *d_matrix, T **matrix, int rows, int cols) {
    T **temp_matrix = new T *[rows * cols];
    for (int i = 0; i < rows; ++i)
        memcpy(temp_matrix + i * cols, matrix[i], cols * sizeof(T));
    cudaMemcpy(d_matrix, temp_matrix, rows * cols * sizeof(T), cudaMemcpyHostToDevice);
    delete[] temp_matrix;
}


template <typename T>
void copy_matrix_from_device(T **matrix, T *d_matrix, int rows, int cols) {
    T **temp_matrix = new T *[rows * cols];
    cudaMemcpy(temp_matrix, d_matrix, rows * cols * sizeof(T), cudaMemcpyDeviceToHost);
    for (int i = 0; i < rows; ++i) {
        memcpy(matrix[i], temp_matrix + i * cols, cols * sizeof(T));
    }
    delete[] temp_matrix;
}


double HiddenMarkovMmodel::run_GPU_forward_algo() {
    for (int i = 0; i < n_states; ++i) {
        alpha[0][i] = initial_distribution[i] * emissions[i][observations[0]];
    }

    double *d_alpha = allocate_matrix_on_device<double>(n_observations, n_states);
    double *d_transitions = allocate_matrix_on_device<double>(n_states, n_states);
    double *d_emissions = allocate_matrix_on_device<double>(n_states, observations_dict_size);
    int *d_observations;
    cudaMalloc(&d_observations, n_observations * sizeof(int));

    copy_matrix_to_device<double>(d_alpha, alpha, n_observations, n_states);
    copy_matrix_to_device<double>(d_transitions, transitions, n_states, n_states);
    copy_matrix_to_device<double>(d_emissions, emissions, n_states, observations_dict_size);
    cudaMemcpy(d_observations, observations, n_observations * sizeof(int), cudaMemcpyHostToDevice);
    cudaDeviceSynchronize();

    for (int t = 1; t < n_observations; ++t) {
        make_forward_step<<<n_states/THREADS_PER_BLOCK+1, THREADS_PER_BLOCK>>>(
                d_alpha, d_transitions, d_emissions, d_observations,
                t, n_states, observations_dict_size);
        cudaDeviceSynchronize();
    }

    copy_matrix_from_device<double>(alpha, d_alpha, n_observations, n_states);

    double likelihood = 0.0;
    for (int i = 0; i < n_states; ++i) {
        likelihood += alpha[n_observations - 1][i];
    }

    cudaFree(d_alpha);
    cudaFree(d_transitions);
    cudaFree(d_emissions);
    cudaFree(d_observations);

    return likelihood;
}


__global__
void make_viterbi_forward_step(double *alpha, double *transitions, double *emissions, int *observations,
                                int t, int n_states, int observations_dict_size) {
    int i = threadIdx.x + blockIdx.x * blockDim.x;
    if (i < n_states) {
        alpha[t * n_states + i] = -1;
        for (int j = 0; j < n_states; ++j) {
            double prob = transitions[j * n_states + i] * emissions[i * observations_dict_size + observations[t]];
            if (alpha[t * n_states + i] < alpha[(t - 1) * n_states + j] * prob) {
                alpha[t * n_states + i] = alpha[(t - 1) * n_states + j] * prob;
            }
        }
    }
}


__global__
void make_viterbi_backward_step(double *alpha, double *transitions, double *backtrack, int t, int n_states) {
    int i = threadIdx.x + blockIdx.x * blockDim.x;
    if (i < n_states) {
        double most_likely_prev_prob = -1;
        for (int j = 0; j < n_states; ++j) {
            if (most_likely_prev_prob < alpha[t * n_states + j] * transitions[j * n_states + i]) {
                most_likely_prev_prob = alpha[t * n_states + j] * transitions[j * n_states + i];
                backtrack[t * n_states + i] = j;
            }
        }
    }
}


void HiddenMarkovMmodel::run_GPU_viterbi_algo(int *hidden_states_idxs) {
    for (int i = 0; i < n_states; ++i) {
        alpha[0][i] = initial_distribution[i] * emissions[i][observations[0]];
    }
    double **backtrack = new double *[n_observations];
    for (int i = 0; i < n_observations; i++) {
        backtrack[i] = new double [n_states];
    }

    double *d_alpha = allocate_matrix_on_device<double>(n_observations, n_states);
    double *d_transitions = allocate_matrix_on_device<double>(n_states, n_states);
    double *d_emissions = allocate_matrix_on_device<double>(n_states, observations_dict_size);
    double *d_backtrack = allocate_matrix_on_device<double>(n_observations, n_states);
    int *d_observations;
    cudaMalloc(&d_observations, n_observations * sizeof(int));

    copy_matrix_to_device<double>(d_alpha, alpha, n_observations, n_states);
    copy_matrix_to_device<double>(d_transitions, transitions, n_states, n_states);
    copy_matrix_to_device<double>(d_emissions, emissions, n_states, observations_dict_size);
    copy_matrix_to_device<double>(d_backtrack, backtrack, n_observations, n_states);
    cudaMemcpy(d_observations, observations, n_observations * sizeof(int), cudaMemcpyHostToDevice);
    cudaDeviceSynchronize();

    for (int t = 1; t < n_observations; ++t) {
        make_viterbi_forward_step<<<n_states/THREADS_PER_BLOCK+1, THREADS_PER_BLOCK>>>(
            d_alpha, d_transitions, d_emissions, d_observations, 
            t, n_states, observations_dict_size);
        cudaDeviceSynchronize();
    }
    for (int t = 0; t < n_observations; ++t) {
        make_viterbi_backward_step<<<n_states/THREADS_PER_BLOCK+1, THREADS_PER_BLOCK>>>(
            d_alpha, d_transitions, d_backtrack, t, n_states);
    }
    
    copy_matrix_from_device<double>(alpha, d_alpha, n_observations, n_states);
    copy_matrix_from_device<double>(backtrack, d_backtrack, n_observations, n_states);
    cudaFree(d_backtrack);
    cudaFree(d_alpha);
    cudaFree(d_transitions);
    cudaFree(d_emissions);
    cudaFree(d_observations);

    // Calculate the most likely last hidden state
    double max_likelihood = -1;
    for (int i = 0; i < n_states; ++i) {
        if (alpha[n_observations - 1][i] > max_likelihood) {
            max_likelihood = alpha[n_observations - 1][i];
            hidden_states_idxs[n_observations - 1] = i;
        }
    }
    // Go backtracking
    for (int i = n_observations - 2; i >= 0; --i) {
        int next_state = hidden_states_idxs[i + 1];
        hidden_states_idxs[i] = (int) backtrack[i + 1][next_state];
    }
    // Free memory
    for (int i = 0; i < n_observations; ++i) {
        delete backtrack[i];
    }
    delete[] backtrack;
}


void HiddenMarkovMmodel::run_GPU_baum_welch_algo(int epochs) {
    generate_random_parameters();
    // Allocate memory
    auto **gamma = new double *[n_observations];
    auto **xi = new double *[n_states];
    for (int i = 0; i < n_observations; ++i) {
        gamma[i] = new double[n_states];
    }
    for (int i = 0; i < n_states; ++i) {
        xi[i] = new double[n_states];
    }
    // Fit the model
    for (int epoch = 0; epoch < epochs; ++epoch) {
        double likelihood = run_GPU_forward_algo();
        if (epoch % 10 == 0) {
            cout << "Epoch: " << epoch << " | Likelihood: " << likelihood << endl;
        }
        // Calculate probabilities: beta, gamma and xi
        for (int i = 0; i < n_states; ++i) {
            beta[n_observations - 1][i] = 1;
            gamma[n_observations - 1][i] = (alpha[n_observations - 1][i] * beta[n_observations - 1][i]) / likelihood;
            memset(xi[i], 0.0, n_states * sizeof(double));
        }
        for (int t = n_observations - 2; t >= 0; --t) {
            for (int i = 0; i < n_states; ++i) {
                beta[t][i] = 0.0;
                for (int j = 0; j < n_states; ++j) {
                    double prob = transitions[i][j] * emissions[j][observations[t + 1]];
                    beta[t][i] += beta[t + 1][j] * prob;
                    xi[i][j] += (alpha[t][i] * prob * beta[t + 1][j]) / likelihood;
                }
                gamma[t][i] = (alpha[t][i] * beta[t][i]) / likelihood;
            }
        }
        // Optimize parameters
        for (int i = 0; i < n_states; ++i) {
            double occupation_prob = 0.0;
            for (int t = 0; t < n_observations - 1; ++t) {
                occupation_prob += gamma[t][i];
            }
            for (int j = 0; j < n_states; ++j) {
                transitions[i][j] = xi[i][j] / occupation_prob;
            }
            double total_occupation_prob = occupation_prob + gamma[n_observations - 1][i];
            for (int j = 0; j < n_observations; ++j) {
                double sum = 0.0;
                for (int t = 0; t < n_observations; ++t) {
                    sum += (observations[t] == observations[j]) ? gamma[t][i] : 0;
                }
                emissions[i][observations[j]] = sum / total_occupation_prob;
            }
            initial_distribution[i] = gamma[0][i];
        }
    }
    // Free memory
    for (int i = 0; i < n_observations; ++i) {
        delete gamma[i];
    }
    delete[] gamma;
    for (int i = 0; i < n_states; ++i) {
        delete xi[i];
    }
    delete[] xi;
}
