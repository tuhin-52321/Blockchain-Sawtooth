#include <stdio.h>
int
main ()
{
#if (TW_DEBUG)
    printf ("Debug \n");
#else
    printf ("Release \n");
#endif
}