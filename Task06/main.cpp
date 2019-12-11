#include <iostream>
#include <random>
#include <iomanip>

using namespace std;

const int N_STATES = 5;
const int N_OBSERVATIONS = 3;
const int OBSERVATIONS_DICT_SIZE = 2;

double transitions[N_STATES][N_STATES];
double emissions[N_STATES][OBSERVATIONS_DICT_SIZE];
double initial_distribution[N_STATES];
int observations[N_OBSERVATIONS];


void generate_distributions(double A[N_STATES][N_STATES],
                            double B[N_STATES][OBSERVATIONS_DICT_SIZE],
                            double pi[N_STATES]) {
    random_device device;
    mt19937 gen(device());
    uniform_int_distribution<> distribution(0, 1000);
    /// Generate stochastic matrix A
    for (int i = 0; i < N_STATES; ++i) {
        double sum = 0;
        for (int j = 0; j < N_STATES; ++j) {
            A[i][j] = distribution(gen);
            sum += A[i][j];
        }
        for (int j = 0; j < N_STATES; ++j) {
            A[i][j] /= sum;
        }
    }
    /// Generate stochastic matrix B
    for (int i = 0; i < N_STATES; ++i) {
        double sum = 0;
        for (int j = 0; j < OBSERVATIONS_DICT_SIZE; ++j) {
            B[i][j] = distribution(gen);
            sum += B[i][j];
        }
        for (int j = 0; j < OBSERVATIONS_DICT_SIZE; ++j) {
            B[i][j] /= sum;
        }
    }
    /// Generate initial probabilities distribution (pi)
    double sum = 0;
    for (int i = 0; i < N_STATES; ++i) {
        pi[i] = distribution(gen);
        sum += pi[i];
    }
    for (int i = 0; i < N_STATES; ++i) {
        pi[i] /= sum;
    }
    /// Generate observations

}


template<typename T>
void generate_observations(T observations[N_OBSERVATIONS], T dict[OBSERVATIONS_DICT_SIZE]) {
    random_device device;
    mt19937 gen(device());
    uniform_int_distribution<> distribution(0, OBSERVATIONS_DICT_SIZE - 1);
    for (int i = 0; i < N_OBSERVATIONS; ++i) {
        observations[i] = dict[distribution(gen)];
    }
}


template<typename T>
void print_hmm(const double A[N_STATES][N_STATES],
               const double B[N_STATES][OBSERVATIONS_DICT_SIZE],
               const double initial[N_STATES],
               const T observations[N_OBSERVATIONS]) {
    cout << "Transitions matrix: \n";
    for (int i = 0; i < N_STATES; ++i) {
        for (int j = 0; j < N_STATES; ++j) {
            cout << A[i][j] << " ";
        }
        cout << endl;
    }
    cout << "Emissions matrix: \n";
    for (int i = 0; i < N_STATES; ++i) {
        for (int j = 0; j < OBSERVATIONS_DICT_SIZE; ++j) {
            cout << B[i][j] << " ";
        }
        cout << endl;
    }
    cout << "Initial distribution: \n";
    for (int i = 0; i < N_STATES; ++i) {
        cout << initial[i] << " ";
    }
    cout << endl;
    cout << "Observations: \n";
    for (int i = 0; i < N_OBSERVATIONS; ++i) {
        cout << observations[i] << " ";
    }
    cout << endl;
}


/// Function runs The Forward Algorithm on Hidden Markov Model
/// @return likelihood of given observations with respect to given HMM
template<typename T>
double run_forward_algo(const double A[N_STATES][N_STATES],
                        const double B[N_STATES][OBSERVATIONS_DICT_SIZE],
                        const double initial[N_STATES],
                        const T observations[N_OBSERVATIONS]) {
    /// alpha[t][i] is the probability of all observations up to time t in the hidden state i
    double alpha[N_OBSERVATIONS][N_STATES];
    for (int i = 0; i < N_STATES; ++i) {
        alpha[0][i] = initial[i] * B[i][observations[0]];
    }
    for (int t = 1; t < N_OBSERVATIONS; ++t) {
        for (int i = 0; i < N_STATES; ++i) {
            alpha[t][i] = 0.0;
            for (int j = 0; j < N_STATES; ++j) {
                alpha[t][i] += alpha[t - 1][j] * A[j][i] * B[i][observations[t]];
            }
        }
    }
    double likelihood = 0.0;
    for (int i = 0; i < N_STATES; ++i) {
        likelihood += alpha[N_OBSERVATIONS - 1][i];
    }
    return likelihood;
}


/// Function runs The Viterbi Algorithm on Hidden Markov Model
/// @return indices of best hidden states sequence
template<typename T>
void run_viterbi_algo(const double A[N_STATES][N_STATES],
                      const double B[N_STATES][OBSERVATIONS_DICT_SIZE],
                      const double initial[N_STATES],
                      const T observations[N_OBSERVATIONS],
                      int hidden_states_idxs[N_OBSERVATIONS]) {
    /// alpha[t][i] is the most likely probability of all observations up to time t in the hidden state i
    double alpha[N_OBSERVATIONS][N_STATES];
    for (int i = 0; i < N_STATES; ++i) {
        alpha[0][i] = initial[i] * B[i][observations[0]];
    }
    int backward[N_OBSERVATIONS][N_STATES];
    for (int t = 1; t < N_OBSERVATIONS; ++t) {
        for (int i = 0; i < N_STATES; ++i) {
            alpha[t][i] = 0.0;
            for (int j = 0; j < N_STATES; ++j) {
                double prob = A[j][i] * B[i][observations[t]];
                if (alpha[t][i] < alpha[t - 1][j] * prob) {
                    backward[t][i] = j;
                    alpha[t][i] = alpha[t - 1][j] * prob;
                }
            }
        }
    }
    /// Calculate most likely last hidden state
    double max_likelihood = 0.0;
    for (int i = 0; i < N_STATES; ++i) {
        if (alpha[N_OBSERVATIONS - 1][i] > max_likelihood) {
            max_likelihood = alpha[N_OBSERVATIONS - 1][i];
            hidden_states_idxs[N_OBSERVATIONS - 1] = i;
        }
    }
    /// Make backward step
    for (int i = N_OBSERVATIONS - 2; i >= 0; --i) {
        int next_state = hidden_states_idxs[i + 1];
        hidden_states_idxs[i] = backward[i + 1][next_state];
    }
}


int main() {
    cout << fixed << setprecision(3);
    /// Create observations dictionary
    /// Methods use templates, so we'll use ints instead of T
    int dict[OBSERVATIONS_DICT_SIZE];
    for (int i = 0; i < OBSERVATIONS_DICT_SIZE; ++i) {
        dict[i] = i;
    }
    generate_distributions(transitions, emissions, initial_distribution);
    generate_observations(observations, dict);
    print_hmm(transitions, emissions, initial_distribution, observations);

    cout << run_forward_algo(transitions, emissions, initial_distribution, observations) << endl;

    int hidden[N_OBSERVATIONS];
    run_viterbi_algo(transitions, emissions, initial_distribution, observations, hidden);
    for (int state : hidden)
        cout << state << " ";
    cout << endl;

    return 0;
}
