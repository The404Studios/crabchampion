#pragma once
#include <string>
#include "SDK.h"

namespace SDKHelper
{
    // -----------------------------
    // UObject array wrapper
    // -----------------------------
    class TUObjectArrayWrapper
    {
    private:
        void* GObjectsAddress = nullptr;

    private:
        void InitGObjects();

    public:
        TUObjectArrayWrapper() = default;
        TUObjectArrayWrapper(const TUObjectArrayWrapper&) = delete;
        TUObjectArrayWrapper(TUObjectArrayWrapper&&) = delete;
        TUObjectArrayWrapper& operator=(const TUObjectArrayWrapper&) = delete;
        TUObjectArrayWrapper& operator=(TUObjectArrayWrapper&&) = delete;

        void InitManually(void* GObjectsAddressParameter);

        SDK::UObject* GetByIndex(int index) const;
        int GetGlobalObjectsCount() const;
        SDK::TUObjectArray* GetTypedPtr();
    };

    // -----------------------------
    // UObject helpers
    // -----------------------------
    SDK::UObject* FindObjectByName(const std::string& fullName, SDK::EClassCastFlags type = SDK::EClassCastFlags::None);
    SDK::UFunction* FindFunctionByName(const std::string& fullName);
    SDK::FVector GetComponentLocationSafe(SDK::USceneComponent* comp);
}

// -----------------------------
// Internal utility namespace
// -----------------------------
namespace SDK::InSDKUtils
{
    uintptr_t GetImageBase();

    template<typename FuncType>
    FuncType GetVirtualFunction(const void* ObjectInstance, int32 Index);

    template<typename FuncType, typename... ParamTypes>
        requires std::invocable<FuncType, ParamTypes...>
    auto CallGameFunction(FuncType Function, ParamTypes&&... Args);
}
