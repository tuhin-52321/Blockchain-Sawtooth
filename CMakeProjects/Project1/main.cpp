#include <stdio.h>
int
main ()
{
#if (TW_DEBUG)
    printf ("TWDebug \n");
#endif
#if (TW_DIAGS)
    printf ("TW_DIAGS\n");
#endif
#if         (TW_SIMULATOR)
    printf ("TW_SIMULATOR\n");
#endif
#if         (TW_METRICS)
    printf ("TW_METRICS\n");
#endif
#if         (TW_TRACE)
    printf ("TW_TRACE\n");
#endif
}