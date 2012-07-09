#ifndef __C_PERFORCE_H__
#define __C_PERFORCE_H__

#ifdef __cplusplus
extern "C" {
#endif

typedef struct p4handle p4handle;

p4handle * p4_connect( char * p4port );

void p4_close( p4handle * ctx );

int p4_login( p4handle* ctx, char * username, char* password );

int p4_get_binarybuf( p4handle *ctx, char * buf, size_t buf_len );

int p4_get_infobuf( p4handle *ctx, char * buf, size_t buf_len );

int p4_get_textbuf( p4handle *ctx, char * buf, size_t buf_len );


void (*p4_get_stat_string_cb)(const char * key );

int p4_get_stat_keys( p4handle *ctx, void(*p4_get_stat_string_cb)(const char*) );

int p4_get_stat_value( p4handle *ctx, const char * key, void(*p4_get_stat_string_cb)(const char*) );

int p4_run( p4handle *ctx, int argc, char ** argv, char * input );

/*
 * return non-zero if we are connected to a perforce server.
 */
int p4_dropped(p4handle * ctx);

#ifdef __cplusplus
}
#endif

#endif

