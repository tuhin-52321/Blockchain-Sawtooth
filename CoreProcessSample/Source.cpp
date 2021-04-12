#include    "TWCoreProcessLib.hpp"

TWSignal    gTWSignal;
TUInt64     gNum;

bool
TWEventHandlerTest (TWSEvent & pEvent, VPtr pParams) noexcept
{

        TWSObject       respobj;
        TWStatusData    status;

    TRACE_LOG ("SENDING RESPONSE \n");

    if (!TWEventHandler::PostResponse(pEvent, status))
        return false;

    return true;
}

TUInt8  gCount = 1;

bool
TWCB(TWSEvent & pEvent, VPtr vParam) noexcept {

    TWAtomics::Increment (gNum);

    TRACE_LOG ("CALLBACK INVOKED\n");
    
    TRACE_VAL (gNum);

    if (gNum == gCount)
        gTWSignal.RaiseSignal ();

    return false;
}

void
TWSEventTest ()
{
        TWSObject   obj;
        TWSObject   respobj;
        TWDUID      duid;
        TWPUID      puid;
        TWPUID      getpuid;
        TWSString    str;
        TInt8 *     buf;
        TUInt16     bufsize;

    duid.Init ();
    TWUtils::MemSet(&puid, 1, sizeof(TWPUID));

    if (!TWDUIDCache::Insert (duid, puid))
        return;

    bufsize = 600;
    buf = (TInt8 *) TWMemoryMgr::MemMalloc (bufsize);
    TWUtils::MemSet (buf, 1, bufsize);
    buf[bufsize] = '\0';


    if (!str.SetString(buf))
        return;

    if (!obj.Initialize (nullptr, nullptr))
        return;

    if (!obj.CreateContent (20, str.GetValue ()))
        return;

    for (TUInt8 i = 0; i < gCount; i++)
        TWEventHandler::PostASync (eTWAPI::TWEVENTHANDLERTEST, obj, TWCB, nullptr);
}

int main ()
{
    TW_DEV_SUPPORT_INITIALIZE;

    if (!TWCoreMain::Initialize (nullptr)) {
        TRACE_LOG("Core init failed");
        TW_DEV_SUPPORT_FINALIZE;
        return 0;
    }

    if (!TWCoreDataMain::Initialize (nullptr)) {
        TRACE_LOG("CoreData init failed");
        TWCoreMain::Finalize ();
        TW_DEV_SUPPORT_FINALIZE;
        return 0;
    }

    if (!TWCoreIOMain::Initialize (nullptr)) {
        TRACE_LOG("CoreIO init failed");
        TWCoreDataMain::Finalize ();
        TWCoreMain::Finalize ();
        TW_DEV_SUPPORT_FINALIZE;

        return 0;
    }

    if (!TWCoreProcessMain::Initialize<true> (nullptr)) {
        TRACE_LOG("CoreProcess init failed");
        TWCoreIOMain::Finalize ();
        TWCoreDataMain::Finalize ();
        TWCoreMain::Finalize ();
        TW_DEV_SUPPORT_FINALIZE;

        return 0;
    }

    if (!gTWSignal.Initialize ())
        return -1;

    TWSEventTest ();

    gTWSignal.GetSignal ();

    while (1)
        TWSLEEP(1);

    TWCoreProcessMain::Finalize<true> ();
    TWCoreIOMain::Finalize ();
    TWCoreDataMain::Finalize ();
    TWCoreMain::Finalize ();
    TW_DEV_SUPPORT_FINALIZE;

    return 0;
}
