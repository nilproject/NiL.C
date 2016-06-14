
#include <stdio.h>
#include <stdlib.h>
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
	short a[6] = { 0,1,2,3,4,5 };
	a[0] = sizeof(int) / 4;
	int i = 1;

	printf("%i", *a);
	getchar();
}