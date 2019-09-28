
void generate_equation_system(size_t N, 
                                double *matrix_A[N],
                                double vector_b[N]);

void solve_SLE_sequentially(size_t N, 
                                double *matrix_A[N], 
                                double vector_b[N], 
                                double vector_x[N]);

void solve_SLE_parallel(size_t N, 
                            double *matrix_A[N], 
                            double vector_b[N], 
                            double vector_x[N]);

void solve_SLE_sequentially_with_gsl(size_t N, 
                                        double matrix_A[N * N], 
                                        double vector_b[N], 
                                        double vector_x[N]);