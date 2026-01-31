#pragma once

#define WIN32_LEAN_AND_MEAN
#define NOMINMAX

#include <string>
#include <fstream>
#include <sstream>
#include <ostream>
#include <vector>
#include <map>
#include <windows.h>
#include <memory>

extern std::string g_fseBasePath;
void InitializeFSEPaths(HMODULE hMod);
std::string GetLogFilePath();
std::string GetQuestsConfigPath();
std::string GetScriptPath(const std::string& scriptFile);

inline void LogToFile(const std::string& message) {
    std::string logPath = GetLogFilePath();
    if (logPath.empty()) return;

    std::ofstream logfile(logPath, std::ios_base::app);
    if (logfile.is_open()) {
        logfile << message << std::endl;
    }
}
extern DWORD g_fableBase;

template<typename T>
inline T ASLR(DWORD absoluteAddress) {
    DWORD defaultFableBase = 0x400000;
    return (T)(g_fableBase + (absoluteAddress - defaultFableBase));
}

template<typename T_Function>
inline void* GetMemberFunctionAddress(T_Function func) {
    union { T_Function func_ptr; void* void_ptr; } converter;
    converter.func_ptr = func;
    return converter.void_ptr;
}

struct CScriptBase_Retail;

inline void AutoRegisterMain(CScriptBase_Retail* pScriptBase, void* pMainFunction);

class CGameScriptInterfaceBase;

struct CScriptDataBase {
    void** pVTable;
};

class LuaQuestHost;
class LuaEntityHost;

struct CPersistContext {

    virtual ~CPersistContext() = default;
};

struct CScriptGameResourceObjectScriptedThingBase;

struct CCPPointerInfo {
    unsigned int RefCount;                          // 0x00
    void(__fastcall* DeleteFunc)(void*);          // 0x04
    void* Data;                                     // 0x08
};

template<typename T>
struct CCountedPointer {
    T* Data;                                        // 0x00
    CCPPointerInfo* Info;                          // 0x04
};

struct CScriptGameResourceObjectBase {
    void** pVTable;                                
    void* BaseData;                                
};

struct CScriptGameResourceObjectScriptedThingBase : CScriptGameResourceObjectBase {

    CCountedPointer<CScriptGameResourceObjectScriptedThingBase> pImp;
};

struct CScriptGameResourceObjectMovieBase : CScriptGameResourceObjectBase {

    CCountedPointer<CScriptGameResourceObjectMovieBase> pImp;  
};

struct CScriptThing {
    void** pVTable;
    CCountedPointer<CScriptGameResourceObjectScriptedThingBase> pImp;
};

extern void** g_pCScriptThingVTable;
void LogCScriptThingDetails(const std::string& context, CScriptThing* pThing);

class CGameScriptInterfaceBase {
public:
    void** pVTable;
};

struct CCharString {
    void* pStringData;
};

struct CSpawnedFunc {
    // --- Base Class Data ---
    void** pVTable;                 // 0x00
    char unknown_base_padding[16];  // 0x04 

    // --- CSpawnedFuncBase Members ---
    unsigned int ID;                // 0x14 - Set by constructor, likely unused
    int LastScriptFrame;            // 0x18 - Set by constructor, likely unused
    int CurrentScriptFrame;         // 0x1C - Set by constructor, likely unused
    CCharString Name;               // 0x20 - Set by constructor

    CCharString AssociatedRegionName; // 0x24 - Set by AddSpawnedFunction

    bool FuncPaused;                // 0x28
    bool Interrupted;               // 0x29
    char unknown_padding[2];        // 0x2A
    void* pPredicateData;           // 0x2C - CCountedPointer Data
    void* pPredicateInfo;           // 0x30 - CCountedPointer Info

    // --- Members set by FSE code ---
    void* pThunkToMain;             // 0x34 
    CScriptBase_Retail* pOwnerScript; // 0x38 
};

struct CScriptBase_Retail {
    void** pVTable;                     // 0x00
    char unknown[0x30];                 // 0x04 (Covers std::list, CVectorMap, and padding up to 0x34)
    unsigned int LastWorldUpdateFrame;  // 0x34 (Based on retail assembly)
    CSpawnedFunc* PActiveThread;        // 0x38 (Based on retail assembly)
    char unknown2[4];                   // 0x3C (Padding to 0x40)
};

typedef void* (__fastcall* tScriptAllocFunc)(CScriptDataBase*, CGameScriptInterfaceBase*);
typedef void* (__fastcall* tEntityScriptAllocFunc)(void* a1, CScriptBase_Retail* parent, CScriptDataBase* pdata, const CScriptThing* thing);

tEntityScriptAllocFunc GetEntityAllocatorForScript(const std::string& scriptName);

struct CScriptInfo {
    void* pAllocFunc;
    tScriptAllocFunc pAllocDataFunc;
    CCharString Name;
    int ID;
    bool MasterScript;
    char padding[3];
};

enum EScriptAIPriority
{
    SCRIPT_AI_PRIORITY_VERY_LOW = 0x0,
    SCRIPT_AI_PRIORITY_LOW = 0x1,
    SCRIPT_AI_PRIORITY_NORMAL = 0x2,
    SCRIPT_AI_PRIORITY_HIGH = 0x3,
    SCRIPT_AI_PRIORITY_VERY_HIGH = 0x4,
    SCRIPT_AI_PRIORITY_HIGHEST = 0x5,
};

enum EScriptActiveStatus {
    ESAS_UNSTARTED = 0,
};

struct CEntityScriptBindingBase {
    void** pVTable;
    CCharString EntityScriptName;
    CScriptBase_Retail* pParentScript;
    char unknown_padding[4];
    tEntityScriptAllocFunc pAllocFunc;
    bool bSomething;
    char unknown_padding2[3];
    int unknown_zero;
};

typedef void* (*t_malloc)(size_t size);
typedef void(*t_free)(void* ptr);
typedef void(__thiscall* tCCharString_Constructor_Literal)(CCharString* This, const char* string, int no_chars);
typedef void(__thiscall* tCCharString_Constructor_Copy)(CCharString* This, const CCharString* other);
typedef void(__thiscall* tCCharString_Destructor)(CCharString* This);
typedef void(__thiscall* tCheckSection)(void* This, CCharString name);
typedef void(__thiscall* tAddScript)(void* This, const CScriptInfo* script_info, CCharString section_name);
typedef void(__fastcall* tSetScriptActiveStatus)(const CCharString* script_name, EScriptActiveStatus new_status);
typedef void(__thiscall* tCScriptBase_Constructor)(CScriptBase_Retail* This);
typedef void(__thiscall* tCSpawnedFunc_Constructor)(CSpawnedFunc* This, const CCharString* name, int unknown_zero);
typedef void(__thiscall* tAddSpawnedFunction)(CScriptBase_Retail* This, CSpawnedFunc* pFunc, const CCharString* sectionName);
typedef void(__thiscall* tShowMessageAndSuspend)(CGameScriptInterfaceBase* This, const CCharString* pMessage, float duration);
typedef void(__fastcall* tRunCutsceneMacro)(const CCharString* macro_name, void* entitymap, void* flagmap, void* input_args, bool setupcond, bool skippable);
typedef void(__thiscall* tStdMap_Constructor)(void* This);
typedef CScriptGameResourceObjectScriptedThingBase* (__thiscall* tStdMap_OperatorBracket)(void* This, const CCharString* pKey);
typedef void(__thiscall* tStdMap_Destructor)(void* This);
typedef void* (__thiscall* tCBaseObject_Constructor)(void* This);
typedef void* (__thiscall* tCBaseObject_AssignmentOperator)(void* This, const void* pOther);
typedef void* (__thiscall* tCBaseObject_Destructor)(void* This);
typedef void(__thiscall* tCleanupPImp)(void* This);
typedef bool(__thiscall* tInitScriptObjectHelper1)(CScriptGameResourceObjectScriptedThingBase* This);
typedef void(__thiscall* tInitScriptObjectHelper2)(CScriptGameResourceObjectScriptedThingBase* This);
typedef void(__thiscall* tAddEntityScriptBinding)(CScriptBase_Retail* This, CEntityScriptBindingBase* pBinding);
typedef void(__thiscall* tPostAddScriptedEntities_CScriptBase)(CScriptBase_Retail* This);
typedef bool(__thiscall* tIsActiveThreadTerminating_Entity)(void* This);
typedef bool(__thiscall* tIsActiveThreadTerminating_Quest)(void* This);
typedef bool(__fastcall* tIsDistanceBetweenThingsUnder)(const CScriptThing* thing1, const CScriptThing* thing2, float dist);
typedef CCharString* (__thiscall* tCCharString_AssignmentOperator)(CCharString* This, const CCharString* pOther);
typedef const char* (__thiscall* tCCharString_ToConstChar)(const CCharString* This);
typedef CCharString* (__fastcall* tCCharString_OperatorPlus)(CCharString* pResult, const char* pLeft, const CCharString* pRight);
typedef int(__thiscall* tGFCharStringToInt)(const CCharString* This);
typedef CCharString* (__fastcall* tGFIntToCharString)(CCharString* pResult, int value);
typedef void(__thiscall* tCPersistContext_Transfer_bool)(CPersistContext* This, const char* name, bool* value, const bool* defaultValue);
typedef void(__thiscall* tCPersistContext_Transfer_int)(CPersistContext* This, const char* name, int* value, const int* defaultValue);
typedef void(__thiscall* tCPersistContext_Transfer_string)(CPersistContext* This, const char* name, CCharString* value, const CCharString* defaultValue);
typedef void(__thiscall* tCPersistContext_Transfer_float)(CPersistContext* This, const char* name, float* value, const float* defaultValue);
typedef void(__thiscall* tCPersistContext_Transfer_uint)(CPersistContext* This, const char* name, unsigned int* value, const unsigned int* defaultValue);
typedef void(__thiscall* tCSGROSTB_Destructor)(void* This);

extern t_malloc                         Game_malloc;
extern tCCharString_Constructor_Literal CCharString_Construct_Literal;
extern tCCharString_Constructor_Copy    CCharString_Construct_Copy;
extern tCCharString_Destructor          CCharString_Destroy;
extern tCheckSection                    CheckSection;
extern tAddScript                       AddScript;
extern tSetScriptActiveStatus           SetScriptActiveStatus_Func;
extern tCScriptBase_Constructor         CScriptBase_Construct;
extern tCSpawnedFunc_Constructor        CSpawnedFunc_Construct;
extern tAddSpawnedFunction              AddSpawnedFunction_func;
extern void* pEmptyDataAlloc;
extern tScriptAllocFunc                 pSunnyvaleDataAlloc_func;
extern CGameScriptInterfaceBase** g_pDSTGame;
extern tRunCutsceneMacro                RunCutsceneMacro_Func;
extern t_free                           Game_free;
extern tAddEntityScriptBinding AddEntityScriptBinding_API;
extern tPostAddScriptedEntities_CScriptBase PostAddScriptedEntities_CScriptBase_API;
extern void** g_pEntityScriptBindingVTable;
extern tIsDistanceBetweenThingsUnder   IsDistanceBetweenThingsUnder_API;
extern tStdMap_Constructor StdMap_Construct_API;
extern tStdMap_OperatorBracket StdMap_OperatorBracket_API;
extern tStdMap_Destructor StdMap_Destroy_API;
extern tCBaseObject_Constructor CBaseObject_Construct_API;
extern tCBaseObject_AssignmentOperator CBaseObject_Assign_API;
extern tCBaseObject_Destructor CBaseObject_Destroy_API;
extern void** g_pMovieObjectVTable;
extern tCleanupPImp CleanupMoviePImp_API;
extern tInitScriptObjectHelper1 InitScriptObjectHelper1_API;
extern tInitScriptObjectHelper2 InitScriptObjectHelper2_API;
extern tIsActiveThreadTerminating_Entity IsActiveThreadTerminating_Entity_API;
extern tIsActiveThreadTerminating_Quest IsActiveThreadTerminating_Quest_API;
extern tCCharString_AssignmentOperator CCharString_Assign_API;
extern tCCharString_ToConstChar        CCharString_ToConstChar_API;
extern tCCharString_OperatorPlus       CCharString_OperatorPlus_API;
extern tGFCharStringToInt              GFCharStringToInt_API;
extern tGFIntToCharString              GFIntToCharString_API;
extern void** g_pCScriptGameResourceObjectScriptedThingBaseVTable;
extern tCPersistContext_Transfer_bool   CPersistContext_Transfer_bool_API;
extern tCPersistContext_Transfer_int    CPersistContext_Transfer_int_API;
extern tCPersistContext_Transfer_string CPersistContext_Transfer_string_API;
extern tCPersistContext_Transfer_float  CPersistContext_Transfer_float_API;
extern tCPersistContext_Transfer_uint   CPersistContext_Transfer_uint_API;
extern tCSGROSTB_Destructor CSGROSTB_Destroy_API;

std::shared_ptr<CScriptThing> WrapScriptThingOutput(CScriptThing* pThingBuffer);

class FableString {
public:
    CCharString m_charString;

    FableString(const char* literal) {
        m_charString = { 0 };
        if (CCharString_Construct_Literal)
            CCharString_Construct_Literal(&m_charString, literal, -1);
    }

    ~FableString() {
        if (m_charString.pStringData && CCharString_Destroy) {
            CCharString_Destroy(&m_charString);
        }
    }

    operator CCharString() const {
        return m_charString;
    }

    operator const CCharString* () const {
        return &m_charString;
    }

    CCharString* get() {
        return &m_charString;
    }

    FableString(const FableString&) = delete;
    FableString& operator=(const FableString&) = delete;
};

inline void AutoRegisterMain(CScriptBase_Retail* pScriptBase, void* pMainFunction) {
    CSpawnedFunc* pSpawnedFunc = (CSpawnedFunc*)Game_malloc(sizeof(CSpawnedFunc));
    if (pSpawnedFunc) {
        FableString mainStr("Main");
        CSpawnedFunc_Construct(pSpawnedFunc, mainStr, 0);
        pSpawnedFunc->pVTable = ASLR<void**>(0x12F5C24);
        pSpawnedFunc->pThunkToMain = pMainFunction;
        pSpawnedFunc->pOwnerScript = pScriptBase;
        FableString classStr("Class");
        AddSpawnedFunction_func(pScriptBase, pSpawnedFunc, classStr);
    }
}

void InitializeFableAPI();