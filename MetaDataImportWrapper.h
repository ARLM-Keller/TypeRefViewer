//==========================================
// Matt Pietrek
//==========================================

#ifndef _COR_H_
#include "cor.h"
#endif

namespace Wheaty
{

namespace UnmanagedMetaDataHelper
{
    class MetaDataImportWrapper
    {
        public:
        MetaDataImportWrapper( LPCSTR pszFileName );
        ~MetaDataImportWrapper();

        IMetaDataImport *           m_pIMetaDataImport;
        IMetaDataAssemblyImport *   m_pIMetaDataAssemblyImport;

        static LPCSTR   TokenTypeName( mdToken );

        private:
        IMetaDataDispenser *    m_pIMetaDataDispenser;
    };
}

}   // end namespace definition
