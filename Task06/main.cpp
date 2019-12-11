#include <iostream>
#include <random>
#include <iomanip>

using namespace std;


template <typename T>
class hidden_markov_model {
    double **transitions;
    double **emissions;
    double *initial_distribution;
    T *observations;
public:
    int n_states;
    int n_observations;
    int observations_dict_size;
    hidden_markov_model(int n_states, int n_observations, int observations_dict_size);
    void generate_distributions();
    void generate_observations(T *dict);
    void print();
    double run_forward_algo();
    void run_viterbi_algo(int *hidden_states_idxs);
};


template <typename T>
hidden_markov_model<T>::hidden_markov_model(int _n_states, int _n_observations, int _observations_dict_size) {
    n_states = _n_states;
    n_observations = _n_observations;
    observations_dict_size = _observations_dict_size;

    transitions = new double *[n_states];
    for (int i = 0; i < this->n_states; i++) {
        transitions[i] = new double [n_states];
    }
    emissions = new double *[n_states];
    for (int i = 0; i < this->n_states; i++) {
        emissions[i] = new double [observations_dict_size];
    }
    initial_distribution = new double [n_states];
    observations = new T [n_observations];
}


template <typename T>
void hidden_markov_model<T>::generate_distributions() {
    random_device device;
    mt19937 gen(device());
    uniform_int_distribution<> distribution(0, 1000);
    /// Generate stochastic matrix A
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
    /// Generate stochastic matrix B
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
    /// Generate initial probabilities distribution (pi)
    double sum = 0;
    for (int i = 0; i < n_states; ++i) {
        initial_distribution[i] = distribution(gen);
        sum += initial_distribution[i];
    }
    for (int i = 0; i < n_states; ++i) {
        initial_distribution[i] /= sum;
    }
}


template<typename T>
void hidden_markov_model<T>::generate_observations(T *dict) {
    random_device device;
    mt19937 gen(device());
    uniform_int_distribution<> distribution(0, observations_dict_size - 1);
    for (int i = 0; i < n_observations; ++i) {
        observations[i] = dict[distribution(gen)];
    }
}


template<typename T>
void hidden_markov_model<T>::print() {
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


template<typename T>
double hidden_markov_model<T>::run_forward_algo() {

    /// Using dynamic programming approach
    /// alpha[t][i] is the probability of all observations up to time t in the hidden state i
    double alpha[n_observations][n_states];
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


template<typename T>
void hidden_markov_model<T>::run_viterbi_algo(int *hidden_states_idxs) {

    /// Using dynamic programming approach
    /// alpha[t][i] is the most likely probability of all observations up to time [t] in the hidden state [i]
    double alpha[n_observations][n_states];
    for (int i = 0; i < n_states; ++i) {
        alpha[0][i] = initial_distribution[i] * emissions[i][observations[0]];
    }
    int backward[n_observations][n_states];
    for (int t = 1; t < n_observations; ++t) {
        for (int i = 0; i < n_states; ++i) {
            alpha[t][i] = 0.0;
            for (int j = 0; j < n_states; ++j) {
                double prob = transitions[j][i] * emissions[i][observations[t]];
                if (alpha[t][i] < alpha[t - 1][j] * prob) {
                    backward[t][i] = j;
                    alpha[t][i] = alpha[t - 1][j] * prob;
                }
            }
        }
    }
    /// Calculate the most likely last hidden state
    double max_likelihood = 0.0;
    for (int i = 0; i < n_states; ++i) {
        if (alpha[n_observations - 1][i] > max_likelihood) {
            max_likelihood = alpha[n_observations - 1][i];
            hidden_states_idxs[n_observations - 1] = i;
        }
    }
    /// Make backward step
    for (int i = n_observations - 2; i >= 0; --i) {
        int next_state = hidden_states_idxs[i + 1];
        hidden_states_idxs[i] = backward[i + 1][next_state];
    }
}


int main() {
    cout << fixed << setprecision(3);
    int n_states = 5;
    int n_observations = 3;
    int observations_dict_size = 2;

    /// Create observations dictionary
    /// Methods of HMM class use templates, so we'll use ints instead of T
    int dict[observations_dict_size];
    for (int i = 0; i < observations_dict_size; ++i) {
        dict[i] = i;
    }

    /// Create HMM
    hidden_markov_model<int> hmm(n_states, n_observations, observations_dict_size);
    hmm.generate_distributions();
    hmm.generate_observations(dict);
    hmm.print();

    /// Run Forward step
    cout << "Likelihood:" << endl << hmm.run_forward_algo() << endl;

    /// Run Viterbi algorithm
    int hidden[n_observations];
    hmm.run_viterbi_algo(hidden);
    cout << "Hidden states:" << endl;
    for (auto state: hidden) cout << state << " ";
    cout << endl;

    return 0;
}
