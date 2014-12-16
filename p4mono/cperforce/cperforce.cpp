#include <stdio.h>
#include <string.h>
#include <map>
#include <string>

using namespace std;

#include "cperforce.h"

#include "p4/clientapi.h"


class CPerforceUI : public ClientUser {
  public:
    
  StrBuf *infoBuffer;
  StrBuf *textBuffer;
  
  char * binaryData;
  size_t binaryDataLen;
  
  map<string, string> * statData;
  
  CPerforceUI() : ClientUser()
  {
     binaryData = NULL;
     binaryDataLen = 0;
     
     infoBuffer = new StrBuf();
     textBuffer = new StrBuf();

     statData = new map<string,string>();

  }
  
  ~CPerforceUI() 
  {
     if ( binaryData != NULL ) free ( binaryData );

     delete infoBuffer;
     delete textBuffer;
     delete statData;
  }
  
  virtual void OutputInfo(char level, const char * data )
  {
     infoBuffer->Append( data );  
     infoBuffer->Append( "\n" );
     
     fprintf(stderr, "INFO %d:%s\n", infoBuffer->Length(), data );
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
     textBuffer->Append( data, len );
     
     fprintf(stderr, "TEXT %d:%s\n", textBuffer->Length(), data );
  }

// NOTE - pass make method calls on this dict,  dict->GetVar()
  virtual void OutputStat( StrDict * dict )
  {
     
     int i = 0;
     int r;
     
     statData->clear();
     
     do {
        StrRef k;
        StrRef v;     
        r = dict->GetVar( i++, k, v );
        if ( r != 0 ){
           string _key (k.Text());
           string _value (v.Text());
           (*statData)[_key] = _value;      
        }
     } while ( r != 0 );
     
  }
  
  StrBuf inputBuffer;
  
  virtual void InputData( StrBuf * data, Error * err )
  {
     data->Set(inputBuffer);
  }
  
};

extern "C" {

struct p4handle {
  ClientApi * client;
  CPerforceUI * ui;
};


static void clearOutputBufs( p4handle * ctx )
{
    if ( ctx != NULL ){
        if ( ctx->ui->textBuffer != NULL )
            ctx->ui->textBuffer->Clear(); 
        if ( ctx->ui->infoBuffer != NULL )  
            ctx->ui->infoBuffer->Clear();
    }
}

p4handle * p4_connect( char * p4port )
{
    p4handle * ctx = (p4handle*)malloc(1*sizeof(p4handle));
    Error e;
    
    ctx->ui = new CPerforceUI();
    ctx->client = new ClientApi( ctx->ui);
    
    ctx->client->SetProtocol("tag","");
    
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
        clearOutputBufs(ctx);
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
        clearOutputBufs(ctx);
        ctx->client->SetUser( username );
        ctx->client->SetPassword( password );
        
        //ctx->client->Run("login");
        
        return 1;
    }
    return rv;
}

int p4_run( p4handle *ctx, int argc, char ** argv, char * input )
{
    int rv = 0;
    fprintf(stderr,"p4_run %s \n", argv[0]);
    if ( ctx != NULL ) {
        clearOutputBufs(ctx);
        if ( argc > 0 ){
          if ( argc > 1 ){ 
            ctx->client->SetArgv( argc -1, &argv[1] );
          }
          
          if ( input != NULL ) {
            ctx->ui->inputBuffer.Set( input );
          } else {
            ctx->ui->inputBuffer.Clear();
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
        clearOutputBufs(ctx);
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
        if ( ctx->ui->infoBuffer != NULL )
        {
            if ( ctx->ui->infoBuffer->Length() > 0 ){
                strncpy( buf, ctx->ui->infoBuffer->Text(), buf_len - 1 );
            }
            return ctx->ui->infoBuffer->Length();
        }
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

int p4_get_textbuf( p4handle *ctx, char * buf, size_t buf_len )
{
    if ( ctx != NULL )
    {
        if ( ctx->ui->textBuffer != NULL )
        {
            if ( ctx->ui->textBuffer->Length() > 0 )
            {
                strncpy( buf, ctx->ui->textBuffer->Text(), buf_len - 1 );
            }
            return ctx->ui->infoBuffer->Length();
        }
    }
    return -1;
}



// NOTE, caller must free keys 
int p4_get_stat_keys( p4handle *ctx, void (*cb)(const char*) )
{
    int count = 0;
    if ( ctx != NULL ){
        count = ctx->ui->statData->size();
        
        map<string,string>::const_iterator iter;
        int i = 0;    
        for ( iter = ctx->ui->statData->begin() ; iter != ctx->ui->statData->end(); ++iter )
        {
            string _key = iter->first;
            if ( cb != NULL ){
                cb( _key.c_str() );
            }
            i++;
        }
    }
    
    return count;
}

int p4_get_stat_value( p4handle *ctx, const char * key, void(*cb)(const char*) )
{
    int rv = -1;
    if ( ( key != NULL ) && ( cb != NULL ) ) { 

        string _key = (key);
        string _value;
        map<string,string>::const_iterator found = ctx->ui->statData->find( _key );
    
        if ( found != ctx->ui->statData->end() )
        {
            _value = found->second;
            cb( _value.c_str() );
            rv = 1;
        }
    }

    return rv;
}

}