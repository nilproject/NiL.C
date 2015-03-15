
#include <stdio.h>
#include "First.h"

int(*(*f())())[1]
{
	auto t = (int(*(*)()))0;
	auto r = t();
	return 0;
}

typedef int *(*type[])();

int main()
{
	int(*x)(0); // переменная x типа int*
	int(*x2)(int); // функция int x2(int)

	char a = -1;
	printf("%i   %i", 0xfffffffff);
}