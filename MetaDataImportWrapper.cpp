#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include "MetaDataImportWrapper.h"

namespace Wheaty
{

namespace UnmanagedMetaDataHelper
{

MetaDataImportWrapper::MetaDataImportWrapper( LPCSTR pszFileName ) : 
    m_pIMetaDataDispenser( 0 ),
    m_pIMetaDataImport( 0 ),
    m_pIMetaDataAssemblyImport( 0 )
{
    CoInitialize( 0 );

    HRESULT hr;

    // Create the IMetaDataDispenser instance.  We need this to create
    // the IMetaDataImport and IMetaDataAssemblyImport interfaces
    hr = CoCreateInstance(  CLSID_CorMetaDataDispenser, 0,
                            CLSCTX_INPROC_SERVER,
                            IID_IMetaDataDispenser,
                            (LPVOID *)&m_pIMetaDataDispenser );
    if ( FAILED(hr) )
        throw "Unable to create IMetaDataDispenser";

    wchar_t wszFileName[MAX_PATH];
    mbstowcs( wszFileName, pszFileName, lstrlen(pszFileName)+1 );

    // Create the IMetaDataImport interface
    hr = m_pIMetaDataDispenser->OpenScope( wszFileName, ofRead,
                                    IID_IMetaDataImport,
                                    (LPUNKNOWN *)&m_pIMetaDataImport );
    if ( FAILED(hr) )
        throw "Unable to create IID_IMetaDataImport";

    // Create the IMetaDataAssemlyImport interface
    hr = m_pIMetaDataDispenser->OpenScope( wszFileName, ofRead,
                                    IID_IMetaDataAssemblyImport,
                                    (LPUNKNOWN *)&m_pIMetaDataAssemblyImport);
    if ( FAILED(hr) )
        throw "Unable to create IID_IMetaDataAssemblyImport";
}

MetaDataImportWrapper::~MetaDataImportWrapper()
{
    // Clean up our interface instances
    if ( m_pIMetaDataImport )
    {
        m_pIMetaDataImport->Release();
        m_pIMetaDataImport = 0;
    }

    if ( m_pIMetaDataAssemblyImport )
    {
        m_pIMetaDataAssemblyImport->Release();
        m_pIMetaDataAssemblyImport = 0;
    }

    if ( m_pIMetaDataDispenser )
    {
        m_pIMetaDataDispenser->Release();
        m_pIMetaDataDispenser = 0;
    }
}


LPCSTR MetaDataImportWrapper::TokenTypeName( mdToken token )
{
    token = TypeFromToken( token );

#define TokenToName(x) case mdt##x: return #x;

    switch( token )
    {
        TokenToName( Module )
        TokenToName( TypeRef )
        TokenToName( TypeDef )
        TokenToName( FieldDef )
        TokenToName( MethodDef )
        TokenToName( ParamDef )
        TokenToName( InterfaceImpl )
        TokenToName( MemberRef )
        TokenToName( CustomAttribute )
        TokenToName( Permission )
        TokenToName( Signature )
        TokenToName( Event )
        TokenToName( Property )
        TokenToName( ModuleRef )
        TokenToName( TypeSpec )
        TokenToName( Assembly )
        TokenToName( AssemblyRef )
        TokenToName( File )
        TokenToName( ExportedType )
        TokenToName( ManifestResource )
        TokenToName( String )
        TokenToName( Name )
        TokenToName( BaseType )
        default: return "<unknown>";
    }
}

}

}