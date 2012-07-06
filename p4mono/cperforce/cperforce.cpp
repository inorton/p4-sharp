#include "cperforce.h"
#include "CPerforceUI.h"

#include "p4/clientapi.h"

class CPerforceUI : public ClientUser {
  public:
  
  virtual void OutputInfo(char level, const char * data )
  {
  
  }
  
  virtual void OutputBinary( const char* data, int len )
  {
  
  }
  
  virtual void OutputText( const char* data, int len )
  {
  
  }
};

struct p4handle {
  ClientApi * client;
  CPerforceUI * ui;
};



p4handle * p4_connect( char * host, char * port )
{
    p4handle * ctx = (p4handle*)malloc(1*sizeof(p4handle));
    Error e;
    
    ctx->ui = new CPerforceUI();
    ctx->client = new ClientApi( ctx->ui);
    
    ctx->client->SetPort( port );
    ctx->client->SetHost( host );
    
    ctx->client->Init( &e );
    
    if ( e.Test() ){
        fprintf(stderr, "client.Init() failed\n");
        free(ctx);
        ctx = NULL;
    }
    
    return ctx;
}

void p4_close( p4handle * ctx )
{
    if ( ctx != NULL )
    {
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
        
        ctx->client->Run("login");
        
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