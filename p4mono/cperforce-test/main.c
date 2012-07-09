#include <stdio.h>
#include <stdlib.h>

#include "cperforce.h"

static p4handle * p4;

static void print_string( const char* str )
{
    fprintf(stderr, str);
}

static void print_key_value( const char* key )
{
    fprintf(stderr,"%s = ", key );
    p4_get_stat_value( p4, key, &print_string );
    fprintf(stderr,"\n" );
}

int main (int argc, char *argv[])
{
  int i = 0;
  p4 = p4_connect("perforce.ncipher.com:1666");
 
  p4_login( p4, "inb", getenv("P4PASSWD") );
 
  if ( argc > 2 ){
 
    p4_run(p4, argc - 1, argv + 1, NULL);

  } 
  if ( argc == 2 ){
    p4_run(p4, 1, argv + 1, NULL);
  
  }
  
  p4_get_stat_keys( p4, &print_key_value );  
  
  fprintf(stderr, "\n" ); 
  p4_close(p4);
 
  return 0;
}



