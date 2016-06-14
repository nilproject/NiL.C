void main()
{
	int* a = (int*)calloc(10, sizeof(int));
	a[0] = 123;
	a[1] = 456;
	a[2] = a[0] + a[1];
	printf("%i %i %i", *a, a[1], a[2]);
}