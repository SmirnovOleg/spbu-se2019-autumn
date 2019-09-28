#!/bin/bash

touch result.txt
> result.txt
gcc test.c gauss.c -o test -lgsl -lgslcblas -fopenmp -O0

for size_N in {10,100,200,500,1000,2000,3000}
do
    ./test "$size_N" >> result.txt
done