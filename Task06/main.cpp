#include <iostream>
#include <random>
#include <iomanip>
#include <cstring>
#include "hmm.hpp"

using namespace std;


int main() {
    //cout << fixed << setprecision(7);
    const int n_states = 300;
    const int n_observations = 30;
    const int observations_dict_size = 2;
    const int epochs = 100;
    clock_t start_t, end_t;

    // Create observations dictionary
    int dict[observations_dict_size];
    for (int i = 0; i < observations_dict_size; ++i) {
        dict[i] = i;
    }
    // Create HMM
    HiddenMarkovMmodel hmm(n_states, n_observations, observations_dict_size);
    hmm.generate_random_parameters();
    hmm.generate_random_observations(dict);
    cout << "States: " << n_states << " | Observations: " << n_observations << endl << endl;

    // Run Forward
    start_t = clock();
    double likelihood = hmm.run_GPU_forward_algo();
    end_t = clock();
    cout << "Likelihood on GPU: " << likelihood << endl;
    cout << "Forward, time elapsed on GPU: " << double(end_t - start_t) / CLOCKS_PER_SEC << endl << endl;

    start_t = clock();
    likelihood = hmm.run_forward_algo();
    end_t = clock();
    cout << "Likelihood on CPU: " << likelihood << endl;
    cout << "Forward, time elapsed on CPU: " << double(end_t - start_t) / CLOCKS_PER_SEC << endl << endl;


    // Run Viterbi
    int hidden[n_observations];
    start_t = clock();
    hmm.run_GPU_viterbi_algo(hidden);
    end_t = clock();
    cout << "Viterbi, time elapsed on GPU: " << double(end_t - start_t) / CLOCKS_PER_SEC << endl << endl;

    start_t = clock();
    hmm.run_viterbi_algo(hidden);
    end_t = clock();
    cout << "Viterbi, time elapsed on CPU: " << double(end_t - start_t) / CLOCKS_PER_SEC << endl << endl;


    // Run Baum-Welch
    start_t = clock();
    hmm.run_GPU_baum_welch_algo(epochs);
    end_t = clock();
    cout << "Baum-Welch, time elapsed on GPU: " << double(end_t - start_t) / CLOCKS_PER_SEC << endl << endl;

    start_t = clock();
    hmm.run_baum_welch_algo(epochs);
    end_t = clock();
    cout << "Baum-Welch, time elapsed on CPU: " << double(end_t - start_t) / CLOCKS_PER_SEC << endl << endl;

    return 0;
}
