#include <iostream>
#include <random>
#include <iomanip>
#include <cstring>
#include "hmm.hpp"

using namespace std;


double hidden_markov_model::run_forward_algo() {
    // alpha[t][i] is the TOTAL probability
    //      of all observations up to time [t] in the hidden state [i]
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
    // alpha[t][i] is the MOST LIKELY probability 
    //      of all observations up to time [t] in the hidden state [i]
    for (int i = 0; i < n_states; ++i) {
        alpha[0][i] = initial_distribution[i] * emissions[i][observations[0]];
    }
    int **backtrack = new int *[n_observations];
    for (int i = 0; i < n_observations; i++) {
        backtrack[i] = new int [n_states];
    }
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
            double most_likely_prev_prob = -1;
            for (int j = 0; j < n_states; ++j) {
                if (most_likely_prev_prob < alpha[t - 1][j] * transitions[j][i]) {
                    most_likely_prev_prob = alpha[t - 1][j] * transitions[j][i];
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
