#ifndef __LIBTOOLS_PROCESSLIB_MEMORY_PROCESSSEARCHBINARY_H__
#define __LIBTOOLS_PROCESSLIB_MEMORY_PROCESSSEARCHBINARY_H__

#include <Windows.h>
#include <vector>

namespace libTools
{
#ifndef _WIN64
	class CProcessSearchBinary
	{
	private:
		struct SearchBinaryRange
		{
			DWORD dwBeginAddr;
			DWORD dwEndAddr;
		};
	public:
		CProcessSearchBinary() = default;
		~CProcessSearchBinary() = default;
	public:
		// arg
		VOID	Initialize(_In_ DWORD dwMaxSearchCount, _In_ HANDLE hProcess);

	public:
		// 
		DWORD	FindAddr(_In_ LPCSTR lpszCode, _In_ int nOffset, _In_ int nOrderNum, _In_ LPCWSTR lpszModule);

		//
		DWORD	FindCALL(_In_ LPCSTR lpszCode, _In_ int nOffset, _In_ int nMov, _In_ int nOrderNum, _In_ LPCWSTR lpszModule);

		//
		DWORD	FindBase(_In_ LPCSTR lpszCode, _In_ int nOffset, _In_ int nMov, _In_ int nOrderNum, _In_ LPCWSTR lpszModule, DWORD dwAddrLen = 0xFFFFFFFF);

		//
		DWORD	FindBase_ByCALL(_In_ LPCSTR lpszCode, _In_ int nOffset, _In_ int nMov, _In_ int nOrderNum, _In_ LPCWSTR lpszModule, _In_ int nBaseOffset, _In_opt_ DWORD dwAddrLen = 0xFFFFFFFF);

		//
		UINT	FindAddr(_In_ LPCSTR lpszCode, _In_ int nOffset, _In_ LPCWSTR lpszModule, _Out_ std::vector<DWORD>& Vec);
	private:
		//
		BOOL    SetModuleSearchBinaryRange(_In_ LPCWSTR lpszModule);

		//
		BOOL	SearchBase(_In_ LPCSTR szCode, _Out_ std::vector<DWORD>& Vec);

		//
		BOOL	CL_sunday(_In_ CONST DWORD* pKey, _In_ UINT uKeyLen, _In_ BYTE* pCode, _In_ UINT uCodeLen, _Out_ std::vector<int>& vlst);

		//
		int		GetWord_By_Char(_In_ BYTE dwWord, _In_ CONST DWORD* pKey, _In_ UINT uKeyLen);

		//
		BOOL	CompCode(_In_ const DWORD * pCode, _In_ const BYTE * pMem, _In_ UINT uLen);
	private:
		template<typename T>
		T ReadValue(_In_ UINT_PTR Addr)
		{
			T Value = 0;
			::ReadProcessMemory(_hProcess, reinterpret_cast<LPCVOID>(Addr), &Value, sizeof(Value), NULL);
			return Value;
		}
	private:
		DWORD   _dwMaxSearchCount = 10;
		HANDLE  _hProcess = INVALID_HANDLE_VALUE;
	private:
		SearchBinaryRange _SearchRange;
	};
#endif
}


#endif // !__LIBTOOLS_PROCESSLIB_MEMORY_PROCESSSEARCHBINARY_H__
