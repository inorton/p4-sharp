#include <stdio.h>
#include <stdlib.h>

#include "cperforce.h"

int main (int argc, char *argv[])
{

  p4handle * p4 = p4_connect("perforce.ncipher.com:1666");
 
  p4_login( p4, "inb", getenv("P4PASSWD") );
 
  if ( argc > 2 ){
 
    p4_run(p4,argc - 2, argv + 2);
  
  }
  p4_close(p4);
 
  return 0;
}

