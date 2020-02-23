all:
	cd /content/drive/'My Drive'/'Colab Notebooks'/
	nvcc main.cpp hmm_gpu.cu hmm_cpu.cu hmm_utils.cu -o main -std=c++11

test:
	./main
