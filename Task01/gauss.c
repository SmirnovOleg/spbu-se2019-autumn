#include <stdio.h>
#include <gsl/gsl_linalg.h>
#include <omp.h>

void generate_equation_system(size_t N,
                              double *matrix_A[N],
                              double vector_b[N])
{
    for (size_t i = 0; i < N; ++i)
    {
        matrix_A[i] = malloc(N * sizeof(double));
        for (size_t j = 0; j < N; ++j)
        {
            matrix_A[i][j] = rand() % 100;
        }
        vector_b[i] = rand() % 100;
    }
}

void solve_SLE_sequentially(size_t N,
                            double *matrix_A[N],
                            double vector_b[N],
                            double vector_x[N])
{
    // Forward step
    for (int num = 0; num < N - 1; ++num)
    {
        if (matrix_A[num][num] == 0)
        {
            for (int i = num + 1; i < N; ++i)
            {
                if (matrix_A[i][num] != 0)
                {
                    double *temp_vector = matrix_A[num];
                    matrix_A[num] = matrix_A[i];
                    matrix_A[i] = temp_vector;
                    break;
                }
            }
        }
        double pivot = matrix_A[num][num];
        for (int i = num + 1; i < N; ++i)
        {
            double coef = matrix_A[i][num] / pivot;
            for (int j = num; j < N; ++j)
            {
                matrix_A[i][j] -= coef * matrix_A[num][j];
            }
            vector_b[i] -= coef * vector_b[num];
        }
    }
    // Reversed step
    for (int num = N - 1; num >= 0; --num)
    {
        vector_x[num] = vector_b[num];
        for (int i = num + 1; i < N; ++i)
        {
            vector_x[num] -= matrix_A[num][i] * vector_x[i];
        }
        vector_x[num] /= matrix_A[num][num];
    }
}

void solve_SLE_sequentially_with_gsl(size_t N,
                                     double matrix_A[N * N],
                                     double vector_b[N],
                                     double vector_x[N])
{
    gsl_matrix_view gsl_A = gsl_matrix_view_array(matrix_A, N, N);
    gsl_vector_view gsl_b = gsl_vector_view_array(vector_b, N);
    gsl_vector *gsl_x = gsl_vector_alloc(N);
    gsl_permutation *permut = gsl_permutation_alloc(N);
    int signum;
    gsl_linalg_LU_decomp(&gsl_A.matrix, permut, &signum);
    gsl_linalg_LU_solve(&gsl_A.matrix, permut, &gsl_b.vector, gsl_x);
    for (int i = 0; i < N; ++i)
    {
        vector_x[i] = gsl_vector_get(gsl_x, i);
    }
}

void solve_SLE_parallel(size_t N,
                        double *matrix_A[N],
                        double vector_b[N],
                        double vector_x[N])
{
    omp_set_num_threads(omp_get_num_procs());
    // Forward step
    for (int num = 0; num < N - 1; ++num)
    {
        if (matrix_A[num][num] == 0)
        {
            for (int i = num + 1; i < N; ++i)
            {
                if (matrix_A[i][num] != 0)
                {
                    double *temp_vector = matrix_A[num];
                    matrix_A[num] = matrix_A[i];
                    matrix_A[i] = temp_vector;
                    break;
                }
            }
        }
        double pivot = matrix_A[num][num];
        //#pragma omp parallel for default(shared)
        for (int i = num + 1; i < N; ++i)
        {
            double coef = matrix_A[i][num] / pivot;
            #pragma omp simd
            for (int j = num; j < N; ++j)
            {
                matrix_A[i][j] -= coef * matrix_A[num][j];
            }
            vector_b[i] -= coef * vector_b[num];
        }
    }
    // Reversed step
    for (int num = N - 1; num >= 0; --num)
    {
        vector_x[num] = vector_b[num];
        double temp_sum = 0;
        #pragma omp parallel for reduction(+:temp_sum)
        for (int i = num + 1; i < N; ++i)
        {
            temp_sum += matrix_A[num][i] * vector_x[i];
        }
        vector_x[num] -= temp_sum;
        vector_x[num] /= matrix_A[num][num];
    }
}