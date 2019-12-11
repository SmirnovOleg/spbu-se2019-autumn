#include <iostream>
#include <random>
#include <iomanip>
#include <cstring>

using namespace std;


template <typename T>
class hidden_markov_model {
    double **transitions;
    double **emissions;
    double *initial_distribution;
    T *observations;
    double **alpha;
    double **beta;
public:
    int n_states;
    int n_observations;
    int observations_dict_size;
    hidden_markov_model(int n_states, int n_observations, int observations_dict_size);
    void generate_random_parameters();
    void generate_random_observations(T *dict);
    void print();
    double run_forward_algo();
    void run_viterbi_algo(int *hidden_states_idxs);
    void run_baum_welch_algo(int epochs);
};


template <typename T>
hidden_markov_model<T>::hidden_markov_model(int _n_states, int _n_observations, int _observations_dict_size) {
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
    observations = new T [n_observations];

    alpha = new double *[n_observations];
    for (int i = 0; i < n_observations; i++) {
        alpha[i] = new double [n_states];
    }
    beta = new double *[n_observations];
    for (int i = 0; i < n_observations; i++) {
        beta[i] = new double [n_states];
    }
}


template <typename T>
void hidden_markov_model<T>::generate_random_parameters() {
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
void hidden_markov_model<T>::generate_random_observations(T *dict) {
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
    /// Calculate the most likely last hidden state
    double max_likelihood = -1;
    for (int i = 0; i < n_states; ++i) {
        if (alpha[n_observations - 1][i] > max_likelihood) {
            max_likelihood = alpha[n_observations - 1][i];
            hidden_states_idxs[n_observations - 1] = i;
        }
    }
    /// Go backtracking
    for (int i = n_observations - 2; i >= 0; --i) {
        int next_state = hidden_states_idxs[i + 1];
        hidden_states_idxs[i] = backtrack[i + 1][next_state];
    }
}


template<typename T>
void hidden_markov_model<T>::run_baum_welch_algo(int epochs) {
    generate_random_parameters();
    for (int epoch = 0; epoch < epochs; ++epoch) {
        /// Make forward step, calculate alpha and likelihood
        double likelihood = run_forward_algo();
        if (epoch % 5 == 0) {
            cout << "Epoch: " << epoch << " | Likelihood: " << likelihood << endl;
        }
        /// Make backward step, calculate beta, gamma and xi
        double gamma[n_observations][n_states];
        double xi[n_states][n_states];
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
        /// Optimize parameters
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
}


int main() {
    //cout << fixed << setprecision(5);
    const int n_states = 100;
    const int n_observations = 30;
    const int observations_dict_size = 2;
    const int epochs = 150;

    /// Create observations dictionary
    /// Methods of HMM class use templates, so we'll use ints instead of T
    int dict[observations_dict_size];
    for (int i = 0; i < observations_dict_size; ++i) {
        dict[i] = i;
    }

    /// Create HMM
    hidden_markov_model<int> hmm(n_states, n_observations, observations_dict_size);
    hmm.generate_random_parameters();
    hmm.generate_random_observations(dict);
    //hmm.print();
    cout << endl;

    /// Run Forward step
    clock_t start = clock();
    double likelihood = hmm.run_forward_algo();
    double total = double(clock() - start) / CLOCKS_PER_SEC;
    cout << "Likelihood (Forward step):" << hmm.run_forward_algo() << endl;
    cout << "Time elapsed: " << total << endl << endl;

    int hidden[n_observations];
    start = clock();
    hmm.run_viterbi_algo(hidden);
    total = double(clock() - start) / CLOCKS_PER_SEC;
    cout << "Hidden states (Viterbi algorithm) calculated." << endl;
    cout << "Time elapsed: " << total << endl << endl;
    //for (auto state: hidden) cout << state << " ";
    //cout << endl;

    cout << "Fitting HMM (Baum-Welch algorithm):" << endl;
    /// Parameters on first iteration will be generated randomly
    start = clock();
    hmm.run_baum_welch_algo(epochs);
    total = double(clock() - start) / CLOCKS_PER_SEC;
    cout << "Time elapsed: " << total << endl;

    return 0;
}
