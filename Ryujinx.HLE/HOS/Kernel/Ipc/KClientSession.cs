using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services;

namespace Ryujinx.HLE.HOS.Kernel.Ipc
{
    class KClientSession : KSynchronizationObject
    {
        public KProcess CreatorProcess { get; }

        private KSession _parent;

        public int ResourceStatus { get; private set; }

        public IpcService Service { get; set; }

        public KClientSession(Horizon system, KSession parent) : base(system)
        {
            _parent = parent;

            ResourceStatus = 1;
        }

        public KernelResult SendSyncRequest(ulong customCmdBuffAddr = 0, ulong customCmdBuffSize = 0)
        {
            KThread currentThread = System.Scheduler.GetCurrentThread();

            KSessionRequest request = new KSessionRequest(currentThread, customCmdBuffAddr, customCmdBuffSize);

            currentThread.IncrementReferenceCount();

            System.CriticalSection.Enter();

            currentThread.SignaledObj   = null;
            currentThread.ObjSyncResult = KernelResult.Success;

            KernelResult result = _parent.ServerSession.EnqueueRequest(request);

            System.CriticalSection.Leave();

            if (result == KernelResult.Success)
            {
                result = currentThread.ObjSyncResult;
            }

            return result;
        }

        protected override void Destroy()
        {
            _parent.DecrementReferenceCount();
        }
    }
}