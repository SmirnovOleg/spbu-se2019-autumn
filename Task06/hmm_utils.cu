#include <iostream>
#include <random>
#include <iomanip>
#include <cstring>
#include "hmm.hpp"

using namespace std;


HiddenMarkovModel::HiddenMarkovModel(int _n_states, int _n_observations, int _observations_dict_size) {
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


void HiddenMarkovModel::generate_random_parameters() {
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


void HiddenMarkovModel::generate_random_observations(int *dict) {
    random_device device;
    mt19937 gen(device());
    uniform_int_distribution<> distribution(0, observations_dict_size - 1);
    for (int i = 0; i < n_observations; ++i) {
        observations[i] = dict[distribution(gen)];
    }
}


void HiddenMarkovModel::print() {
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
