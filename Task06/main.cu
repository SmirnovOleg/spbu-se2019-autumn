#include <iostream>
#include <random>
#include <iomanip>
#include <cstring>

using namespace std;

const int THREADS_PER_BLOCK = 1024;

class hidden_markov_model {

    double **transitions;
    double **emissions;
    double *initial_distribution;
    int *observations;
    double **alpha;
    double **beta;

public:
    int n_states;
    int n_observations;
    int observations_dict_size;

    hidden_markov_model(int n_states, int n_observations, int observations_dict_size);
    void generate_random_parameters();
    void generate_random_observations(int *dict);
    void print();

    double run_forward_algo();
    void run_viterbi_algo(int *hidden_states_idxs);
    void run_baum_welch_algo(int epochs);

    double run_GPU_forward_algo();
    void run_GPU_baum_welch_algo(int epochs);
};


hidden_markov_model::hidden_markov_model(int _n_states, int _n_observations, int _observations_dict_size) {
    n_states = _n_states;
    n_observations = _n_observations;
    observations_dict_size = _observations_dict_size;

    transitions = new double *[n_states];
    for (int i = 0; i < n_states; i++) {
        transitions[i] = new double [n_states];
    }
    emissions = new double *[n_states];
    for (int i = 0; i < n_states; i++) {
        emissions[i] = new double [observations_dict_size];
    }
    initial_distribution = new double [n_states];
    observations = new int [n_observations];

    alpha = new double *[n_observations];
    for (int i = 0; i < n_observations; i++) {
        alpha[i] = new double [n_states];
    }
    beta = new double *[n_observations];
    for (int i = 0; i < n_observations; i++) {
        beta[i] = new double [n_states];
    }
}


void hidden_markov_model::generate_random_parameters() {
    random_device device;
    mt19937 gen(device());
    uniform_int_distribution<> distribution(0, 1000);
    // Generate stochastic matrix A
    for (int i = 0; i < n_states; ++i) {
        double sum = 0;
        for (int j = 0; j < n_states; ++j) {
            transitions[i][j] = distribution(gen);
            sum += transitions[i][j];
        }
        for (int j = 0; j < n_states; ++j) {
            transitions[i][j] /= sum;
        }
    }
    // Generate stochastic matrix B
    for (int i = 0; i < n_states; ++i) {
        double sum = 0;
        for (int j = 0; j < observations_dict_size; ++j) {
            emissions[i][j] = distribution(gen);
            sum += emissions[i][j];
        }
        for (int j = 0; j < observations_dict_size; ++j) {
            emissions[i][j] /= sum;
        }
    }
    // Generate initial probabilities distribution (pi)
    double sum = 0;
    for (int i = 0; i < n_states; ++i) {
        initial_distribution[i] = distribution(gen);
        sum += initial_distribution[i];
    }
    for (int i = 0; i < n_states; ++i) {
        initial_distribution[i] /= sum;
    }
}


void hidden_markov_model::generate_random_observations(int *dict) {
    random_device device;
    mt19937 gen(device());
    uniform_int_distribution<> distribution(0, observations_dict_size - 1);
    for (int i = 0; i < n_observations; ++i) {
        observations[i] = dict[distribution(gen)];
    }
}


void hidden_markov_model::print() {
    cout << "Transitions matrix: \n";
    for (int i = 0; i < n_states; ++i) {
        for (int j = 0; j < n_states; ++j) {
            cout << transitions[i][j] << " ";
        }
        cout << endl;
    }
    cout << "Emissions matrix: \n";
    for (int i = 0; i < n_states; ++i) {
        for (int j = 0; j < observations_dict_size; ++j) {
            cout << emissions[i][j] << " ";
        }
        cout << endl;
    }
    cout << "Initial distribution: \n";
    for (int i = 0; i < n_states; ++i) {
        cout << initial_distribution[i] << " ";
    }
    cout << endl;
    cout << "Observations: \n";
    for (int i = 0; i < n_observations; ++i) {
        cout << observations[i] << " ";
    }
    cout << endl;
}


double hidden_markov_model::run_forward_algo() {
    // alpha[t][i] is the probability of all observations up to time t in the hidden state i
    for (int i = 0; i < n_states; ++i) {
        alpha[0][i] = initial_distribution[i] * emissions[i][observations[0]];
    }
    for (int t = 1; t < n_observations; ++t) {
        for (int i = 0; i < n_states; ++i) {
            alpha[t][i] = 0.0;
            for (int j = 0; j < n_states; ++j) {
                alpha[t][i] += alpha[t - 1][j] * transitions[j][i] * emissions[i][observations[t]];
            }
        }
    }
    double likelihood = 0.0;
    for (int i = 0; i < n_states; ++i) {
        likelihood += alpha[n_observations - 1][i];
    }
    return likelihood;
}


void hidden_markov_model::run_viterbi_algo(int *hidden_states_idxs) {
    // alpha[t][i] is the most likely probability of all observations up to time [t] in the hidden state [i]
    for (int i = 0; i < n_states; ++i) {
        alpha[0][i] = initial_distribution[i] * emissions[i][observations[0]];
    }
    int backtrack[n_observations][n_states];
    for (int t = 1; t <= n_observations; ++t) {
        for (int i = 0; i < n_states; ++i) {
            if (t != n_observations) {
                alpha[t][i] = -1;
                for (int j = 0; j < n_states; ++j) {
                    double prob = transitions[j][i] * emissions[i][observations[t]];
                    if (alpha[t][i] < alpha[t - 1][j] * prob) {
                        alpha[t][i] = alpha[t - 1][j] * prob;
                    }
                }
            }
            double temp = -1;
            for (int j = 0; j < n_states; ++j) {
                if (temp < alpha[t - 1][j] * transitions[j][i]) {
                    temp = alpha[t - 1][j] * transitions[j][i];
                    backtrack[t - 1][i] = j;
                }
            }
        }
    }
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
        hidden_states_idxs[i] = backtrack[i + 1][next_state];
    }
}


void hidden_markov_model::run_baum_welch_algo(int epochs) {
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
        double likelihood = run_forward_algo();
        /*if (epoch % 5 == 0) {
            cout << "Epoch: " << epoch << " | Likelihood: " << likelihood << endl;
        }*/
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


__global__
void make_forward_step(double *alpha, double *transitions, double *emissions, int *observations,
                       int t, int n_states, int observations_dict_size) {
    int i = threadIdx.x + blockIdx.x * blockDim.x;
    if (i < n_states) {
        alpha[t * n_states + i] = 0.0;
        for (int j = 0; j < n_states; ++j) {
            alpha[t * n_states + i] += alpha[(t - 1) * n_states + j] * transitions[j * n_states + i] * emissions[i * observations_dict_size + observations[t]];
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


double hidden_markov_model::run_GPU_forward_algo() {
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


void hidden_markov_model::run_GPU_baum_welch_algo(int epochs) {
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
        /*if (epoch % 5 == 0) {
            cout << "Epoch: " << epoch << " | Likelihood: " << likelihood << endl;
        }*/
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


int main() {
    //cout << fixed << setprecision(7);
    for (int n_states = 10; n_states < 10000; n_states *= 2) {
        const int n_observations = 300;
        const int observations_dict_size = 2;
        const int epochs = 100;
        clock_t start_t, end_t;

        // Create observations dictionary
        int dict[observations_dict_size];
        for (int i = 0; i < observations_dict_size; ++i) {
            dict[i] = i;
        }
        // Create HMM
        hidden_markov_model hmm(n_states, n_observations, observations_dict_size);
        hmm.generate_random_parameters();
        hmm.generate_random_observations(dict);
        //hmm.print();
        //cout << "States: " << n_states << " | Observations: " << n_observations << endl << endl;
        cout << "[" << n_states << ", ";

        start_t = clock();
        hmm.run_GPU_baum_welch_algo(epochs);
        end_t = clock();
        cout << double(end_t - start_t) / CLOCKS_PER_SEC << ", ";
        //cout << "Likelihood: " << likelihood << " | ";
        //cout << "Time elapsed on GPU: " << double(end_t - start_t) / CLOCKS_PER_SEC << endl << endl;

        start_t = clock();
        hmm.run_baum_welch_algo(epochs);
        end_t = clock();
        cout << double(end_t - start_t) / CLOCKS_PER_SEC << "], " << endl;
        //cout << "Likelihood: " << likelihood << " | ";
        //cout << "Time elapsed on CPU: " << double(end_t - start_t) / CLOCKS_PER_SEC << endl << endl;
    }

/*
    int hidden[n_observations];
    start_t = clock();
    hmm.run_viterbi_algo(hidden);
    end_t = clock();
    cout << "Hidden states (Viterbi algorithm) calculated." << endl;
    cout << "Time elapsed: " << double(end_t - start_t) / CLOCKS_PER_SEC << endl << endl;

    cout << "Fitting HMM (Baum-Welch algorithm):" << endl;
    start_t = clock();
    hmm.run_baum_welch_algo(epochs);
    end_t = clock();
    cout << "Time elapsed: " << double(end_t - start_t) / CLOCKS_PER_SEC << endl << endl;
*/
    return 0;
}
