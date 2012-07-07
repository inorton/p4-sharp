#include <stdio.h>
#include <string.h>

#include "cperforce.h"

#include "CPerforceUI.h"

#include "p4/clientapi.h"

class CPerforceUI : public ClientUser {
  public:
    
  StrBuf infoBuffer;
  StrBuf textBuffer;
  
  char * binaryData;
  size_t binaryDataLen;
  
  StrDict * statDict;
  
  CPerforceUI() : ClientUser()
  {
     binaryData = NULL;
     binaryDataLen = 0;
  }
  
  virtual void OutputInfo(char level, const char * data )
  {
     infoBuffer.Clear();
     infoBuffer.Append( data );  
     
     fprintf(stderr, "INFO:%s\n", data );
  }
  
  virtual void OutputBinary( const char* data, int len )
  {
     if ( binaryData != NULL ) {
        free(binaryData);
        binaryDataLen = len;
        binaryData = (char*)malloc( len * sizeof(char));
        memcpy(binaryData, data, len);
     }
  }
  
  virtual void OutputText( const char* data, int len )
  {
     textBuffer.Clear();
     textBuffer.Append( data, len );
  }

// NOTE - pass make method calls on this dict,  dict->GetVar()
  virtual void OutputStat( StrDict * dict )
  {
     statDict = dict;
  }
  
  
};

extern "C" {

struct p4handle {
  ClientApi * client;
  CPerforceUI * ui;
};



p4handle * p4_connect( char * p4port )
{
    p4handle * ctx = (p4handle*)malloc(1*sizeof(p4handle));
    Error e;
    
    ctx->ui = new CPerforceUI();
    ctx->client = new ClientApi( ctx->ui);
    
    //ctx->client->SetProtocol("tag","");
    
    fprintf(stderr, "in p4_connect( %s )\n", p4port );
    
    ctx->client->SetPort( p4port );
    
    ctx->client->Init( &e );
    
    if ( e.Test() ){
        StrBuf b;
        fprintf(stderr, "client.Init() failed\n");
        e.Fmt(&b);
        fprintf(stderr, "error :\n%s\n", b.Text() );
        free(ctx);
        ctx = NULL;
    }
    
    return ctx;
}

void p4_close( p4handle * ctx )
{
    if ( ctx != NULL )
    {
        Error e;
        if ( ctx->ui->binaryData != NULL)
            free(ctx->ui->binaryData);
    
        ctx->client->Final(&e);
    
        delete ctx->client; 
        delete ctx->ui;
        free(ctx);
    }
}

int p4_login( p4handle* ctx, char * username, char* password )
{
    int rv = 0;
    if ( ctx != NULL ) {
        ctx->client->SetUser( username );
        ctx->client->SetPassword( password );
        
        //ctx->client->Run("login");
        
        return 1;
    }
    return rv;
}

int p4_run( p4handle *ctx, int argc, char ** argv )
{
    int rv = 0;
    if ( ctx != NULL ) {
        if ( argc > 0 ){
          if ( argc > 1 ){ 
            ctx->client->SetArgv( argc -1, &argv[1] );
          }
        
          ctx->client->Run( argv[0] );        
        }
    }
    return rv;
}

int p4_dropped(p4handle * ctx)
{
    int rv = 0;
    
    if ( ctx != NULL )
    {
        if ( !ctx->client->Dropped() )
        {
            return 1;
        }
    }
    return rv;
}

int p4_get_infobuf( p4handle *ctx, char * buf, size_t buf_len )
{
    if ( ctx != NULL )
    {
        size_t info_len = (size_t) strlen(ctx->ui->infoBuffer.Text()) + 1;
        if ( buf_len == 0 ){
            return info_len;
        }
        int count = ( info_len < buf_len ) ? info_len : buf_len; 
        char * dst = buf;
        strncpy( dst, ctx->ui->infoBuffer.Text(), count - 1 );
        
        dst[count-1] = 0x0;
        return count;
    }
    return -1;
}

int p4_get_binarybuf( p4handle *ctx, char * buf, size_t buf_len )
{
    if ( ctx != NULL )
    {
        if ( buf_len == 0 ){
            return ctx->ui->binaryDataLen;
        }
    
        int count = ( buf_len < ctx->ui->binaryDataLen ) ? buf_len : ctx->ui->binaryDataLen ;
        
        memcpy( buf, ctx->ui->binaryData, count );
        
        return count;
    }
    return -1;
}


}