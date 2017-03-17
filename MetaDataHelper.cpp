// This is the main DLL file.

#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include "MetaDataImportWrapper.h"
#define ARRAYSIZE( x )  (sizeof(x) / sizeof(x[0]))

#using <mscorlib.dll>
using namespace System;
using namespace System::Runtime::InteropServices;

namespace Wheaty
{

namespace UnmanagedMetaDataHelper
{
    __gc public class MemberRefInfo
    {
        public:
        String *    m_strMemberName;
        Byte        m_signatureBlob[];
    };

    __gc public class TypeRefInfo
    {
        public:


        Int32           m_mdTypeRef;
        String*         m_strTypeName;
        String*         m_strAssemblyName;
        MemberRefInfo*  m_arMemberRefs[];

        TypeRefInfo(){  m_mdTypeRef = 0;
                        m_strTypeName = S"";
                        m_strAssemblyName = S"";
                        m_arMemberRefs = 0; }
    };

    typedef TypeRefInfo* TYPEREFINFO [];

    __gc public class TypeRefInfoHelper
    {
        public:

        TYPEREFINFO public GetTypeRefInfo( String * strFilename )
        {
            MetaDataImportWrapper * pMetaDataWrapper = 0;
            IntPtr memAnsiIntPtr = 0;

            try
            {
                memAnsiIntPtr = Marshal::StringToCoTaskMemAnsi( strFilename );
                char *pszFilename = (char *)memAnsiIntPtr.ToPointer();

                // New a CMetaDataImportHelper class.  This lets us
                // catch the exception if the metadata isn't loaded
                // properly by the constructor          
                
                pMetaDataWrapper = new MetaDataImportWrapper( pszFilename );

                // Free this up, now that we don't need it any more
                Marshal::FreeCoTaskMem( memAnsiIntPtr );
                memAnsiIntPtr = 0;

                IMetaDataImport * pIMetaData =
                                        pMetaDataWrapper->m_pIMetaDataImport;

                HCORENUM    hEnum = 0;
                mdTypeDef   rTypeRefs[2048];
                ULONG       cTypeRefs = ARRAYSIZE(rTypeRefs);
                
                HRESULT hr = pIMetaData->EnumTypeRefs(  &hEnum,
                                                        rTypeRefs,
                                                        cTypeRefs,
                                                        &cTypeRefs );
                if ( FAILED(hr) )
                    return 0;

                // We don't need the HCORENUM open anymore
                pIMetaData->CloseEnum( hEnum );

                TypeRefInfo * arTypeRefs[] = new TypeRefInfo* [cTypeRefs];
                for ( unsigned i = 0; i < cTypeRefs; i++ )
                {
                    arTypeRefs[i] = new TypeRefInfo;
                    arTypeRefs[i]->m_mdTypeRef = rTypeRefs[i];

                    wchar_t wszTypeRef[512];
                    ULONG   cchTypeRef = ARRAYSIZE(wszTypeRef);
                    mdToken tkResolutionScope;

                    HRESULT hr =
                    pIMetaData->GetTypeRefProps(    rTypeRefs[i],
                                                    &tkResolutionScope,
                                                    wszTypeRef,
                                                    cchTypeRef,
                                                    &cchTypeRef);
                    if ( FAILED(hr) )
                        continue;

                    wchar_t wszAssemblyName[512] = { 0 };
                    ULONG   cchAssemblyName = ARRAYSIZE(wszAssemblyName);

                    if ( TypeFromToken(tkResolutionScope) == mdtAssemblyRef )
                    {
                        pMetaDataWrapper->m_pIMetaDataAssemblyImport->
                                GetAssemblyRefProps(
                                                    tkResolutionScope,
                                                    0,
                                                    0,
                                                    wszAssemblyName,
                                                    cchAssemblyName,
                                                    &cchAssemblyName,
                                                    0,
                                                    0,
                                                    0,
                                                    0 );

                        if ( FAILED(hr) )
                            continue;
                    }

                    arTypeRefs[i]->m_strTypeName = wszTypeRef;
                    arTypeRefs[i]->m_strAssemblyName = wszAssemblyName;

                    // Now spin through all the member refs of this type
                    hEnum = 0;
                    mdMemberRef rMemberRefs[2048];
                    ULONG cMemberRefs = ARRAYSIZE(rMemberRefs);
                    
                    hr = pIMetaData->EnumMemberRefs(&hEnum,
                                                    rTypeRefs[i],
                                                    rMemberRefs,
                                                    cMemberRefs,
                                                    &cMemberRefs );
                    if ( FAILED(hr) )
                        continue;

                    // We don't need the HCORENUM open anymore
                    pIMetaData->CloseEnum( hEnum );
                    
                    arTypeRefs[i]->m_arMemberRefs =
                                        new MemberRefInfo * [ cMemberRefs ];

                    // Spin through all the MemberRefs
                    for ( ULONG j = 0; j < cMemberRefs; j++ )
                    {
                        wchar_t wszMember[512];
                        ULONG   cchMember = ARRAYSIZE(wszMember);

                        PCCOR_SIGNATURE pvSigBlob;
                        ULONG cbSig;

                        // Get member name and signature blob
                        hr = pIMetaData->GetMemberRefProps(
                                        rMemberRefs[j],
                                        0,
                                        wszMember,
                                        cchMember,
                                        &cchMember,
                                        &pvSigBlob,
                                        &cbSig );
                        if ( FAILED(hr) )
                            continue;
                        
                        arTypeRefs[i]->m_arMemberRefs[j] = new MemberRefInfo;
                        arTypeRefs[i]->m_arMemberRefs[j]->m_strMemberName
                                                                = wszMember;

                        // Create a managed Byte array to hold the
                        // signature blob
                        Byte mgdSigBlob[] = new Byte[cbSig];

                        // Copy the bytes from the unmanaged array to the
                        // managed array
                        for ( ULONG k = 0; k < cbSig; k++ )
                            mgdSigBlob[k] = *pvSigBlob++;

                        arTypeRefs[i]->m_arMemberRefs[j]->m_signatureBlob
                                                            = mgdSigBlob;
                    }

                }

                // Return all our results to the caller
                return arTypeRefs;
            
            }
            catch( ... )    // Ooops!
            {
                if ( memAnsiIntPtr != 0 )
                    Marshal::FreeCoTaskMem( memAnsiIntPtr );
            }

            delete pMetaDataWrapper;
                
            return 0;
        }

    };  // end class definition
};

}   // end namespace definition

