#include <stdio.h>
#include <gsl/gsl_linalg.h>
#include <omp.h>
#include <time.h>
#include <string.h>
#include "gauss.h"

#define EXPERIMENTS 20
#define EPS 1e-5

int N;
double **matrix_A, *vector_b, *expected_vector_x;
double **copied_matrix_A, *copied_vector_b, *vector_x1, *vector_x2;
double *flatten_matrix_A;

enum gauss_algo
{
    BUILTIN,
    SEQUENTIAL,
    PARALLEL
};

double timeit(void (*function)(), enum gauss_algo type)
{
    double total_time = 0;
    for (int i = 0; i < EXPERIMENTS; ++i)
    {
        // Create local copies, because gauss transitions corrupt given matrix
        if (type != BUILTIN)
        {
            for (int i = 0; i < N; ++i)
            {
                copied_matrix_A[i] = malloc(N * sizeof(double));
                memcpy(copied_matrix_A[i], matrix_A[i], N * sizeof(double));
            }
        }
        else
        {
            // Create flatten matrix for builtin library
            for (int i = 0; i < N * N; ++i)
            {
                flatten_matrix_A[i] = matrix_A[i / N][i % N];
            }
        }
        memcpy(copied_vector_b, vector_b, N * sizeof(double));

        // Measure time
        clock_t start = clock();
        switch (type)
        {
        case BUILTIN:
            function(N, flatten_matrix_A, copied_vector_b, expected_vector_x);
            break;
        case SEQUENTIAL:
            function(N, copied_matrix_A, copied_vector_b, vector_x1);
            break;
        case PARALLEL:
            function(N, copied_matrix_A, copied_vector_b, vector_x2);
            break;
        default:
            break;
        }
        clock_t end = clock();
        total_time += ((double)(end - start)) / CLOCKS_PER_SEC;
    }
    return total_time / EXPERIMENTS;
}

_Bool compare_vectors(double actual[N], double expected[N])
{
    for (int i = 0; i < N; ++i)
    {
        if (abs(actual[i] - expected[i]) > EPS)
        {
            return 0;
        }
    }
    return 1;
}

int main(int argc, char *argv[])
{
    if (argc != 2)
    {
        exit(1);
    }

    N = atoi(argv[1]);
    if (N <= 0)
    {
        exit(1);
    }

    matrix_A = malloc(N * sizeof(double *));
    copied_matrix_A = malloc(N * sizeof(double *));
    vector_b = malloc(N * sizeof(double));
    copied_vector_b = malloc(N * sizeof(double));
    vector_x1 = malloc(N * sizeof(double));
    vector_x2 = malloc(N * sizeof(double));
    expected_vector_x = malloc(N * sizeof(double));
    flatten_matrix_A = malloc(N * N * sizeof(double));
    srand(time(NULL));

    generate_equation_system(N, matrix_A, vector_b);

    printf("Matrix: %d x %d; %d experiments:\n\n", N, N, EXPERIMENTS);
    printf("       Average builtin function time: %f\n", timeit(solve_SLE_sequentially_with_gsl, BUILTIN));
    printf("    Average sequential function time: %f\n", timeit(solve_SLE_sequentially, SEQUENTIAL));
    printf("      Average parallel function time: %f\n\n", timeit(solve_SLE_parallel, PARALLEL));

    if (compare_vectors(vector_x1, expected_vector_x) && compare_vectors(vector_x2, expected_vector_x))
    {
        printf("    Correct answer\n\n\n");
    }
    else
    {
        printf("    Wrong answer\n\n\n");
    }

    return 0;
}
