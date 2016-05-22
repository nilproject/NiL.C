
#include <stdio.h>
#include <math.h>
#include "First.h"

#define magic(a) 2
#define def

int(*(*f())())[1 + 1]
{
	auto t = (int(*)())0;
	auto tt = (int(*)[])0;
	auto r = t();
	return 0;
}

typedef int *(*type[])();

int main()
{
	def(/* эта скобка не обрабатывается препроцессором */1);
	int(*x); // переменная x типа int*
	int(*x2)(int); // функция int x2(int)
	int(*x3(int));
	goto label;
	magic(int x(f,b) { ну нифига себе });
	label:
	auto a = (void(*)())0;
	printf("%i   %i", 0xfffffffff);
}