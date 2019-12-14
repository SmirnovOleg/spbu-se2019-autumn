class HiddenMarkovMmodel {
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

    HiddenMarkovMmodel(int n_states, int n_observations, int observations_dict_size);
    void generate_random_parameters();
    void generate_random_observations(int *dict);
    void print();

    double run_forward_algo();
    void run_viterbi_algo(int *hidden_states_idxs);
    void run_baum_welch_algo(int epochs);

    double run_GPU_forward_algo();
    void run_GPU_viterbi_algo(int *hidden_states_idxs);
    void run_GPU_baum_welch_algo(int epochs);
};