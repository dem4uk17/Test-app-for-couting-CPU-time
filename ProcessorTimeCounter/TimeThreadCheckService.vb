Imports System.Runtime.InteropServices

Public Class TimeThreadCheckService
    Private listSnapshots As New List(Of Snapshot)
    Sub New()
        System.Threading.Tasks.Task.Factory.StartNew(New Action(AddressOf TimeProcessor))
    End Sub

    Private Sub TimeProcessor()
        While (True)
            SyncLock (listSnapshots)
                Dim snapshotsForDel = New List(Of Snapshot)

                For Each item In listSnapshots
                    Dim kernelTime, userTime, startTime, endTime, retcode As Long

                    retcode = GetThreadTimes(item.IntPtr, startTime, endTime, kernelTime, userTime)

                    If endTime = 0 Then Continue For

                    Dim success = Convert.ToBoolean(retcode)
                    If Not success Then Throw New Exception(String.Format("Failed to get timestamp. error code: {0}", retcode))

                    item.KernelTime = TimeSpan.FromTicks(kernelTime)
                    item.UserTime = TimeSpan.FromTicks(userTime)

                    Dim elapsedMilliseconds As Long = item.KernelTime.Milliseconds + item.UserTime.Milliseconds
                    Console.WriteLine($"Time of thread #{item.CustomThreadID} - {elapsedMilliseconds}")

                    snapshotsForDel.Add(item)
                Next

                listSnapshots.RemoveAll(Function(x) snapshotsForDel.Contains(x))
            End SyncLock

            System.Threading.Thread.Sleep(1000)
        End While
    End Sub

    Public Sub SetForCheckCurrentThread(customThreadID As Integer)
        Dim _threadHandle As IntPtr
        Dim processHandle = GetCurrentProcess()
        Dim result = DuplicateHandle(processHandle, GetCurrentThread(), processHandle, _threadHandle, 0, False, DuplicateOptions.DUPLICATE_SAME_ACCESS)

        SyncLock (listSnapshots)
            listSnapshots.Add(New Snapshot() With {.IntPtr = _threadHandle, .CustomThreadID = customThreadID})
        End SyncLock
    End Sub

    <DllImport("kernel32.dll", CharSet:=CharSet.Auto, SetLastError:=True)>
    Public Shared Function GetThreadTimes(threadHandle As IntPtr, <Out()> ByRef creationTime As Long,
            <Out()> ByRef exitTime As Long, <Out()> ByRef kernelTime As Long, <Out()> ByRef userTime As Long) As Long
    End Function
    <DllImport("kernel32.dll")>
    Public Shared Function GetCurrentThread() As IntPtr
    End Function

    <DllImport("kernel32.dll", CharSet:=CharSet.Auto, SetLastError:=True)>
    Public Shared Function GetCurrentProcess() As IntPtr
    End Function

    <DllImport("kernel32.dll", SetLastError:=True)>
    Public Shared Function DuplicateHandle(hSourceProcessHandle As IntPtr,
        hSourceHandle As IntPtr, hTargetProcessHandle As IntPtr, <Out()> ByRef lpTargetHandle As IntPtr,
        dwDesiredAccess As UInteger, <MarshalAs(UnmanagedType.Bool)> bInheritHandle As Boolean, dwOptions As UInteger) As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function
End Class

<Flags>
Public Enum DuplicateOptions As UInteger
    DUPLICATE_CLOSE_SOURCE = &H1 '0x00000002 Closes the source handle. This occurs regardless of any error status returned.
    DUPLICATE_SAME_ACCESS = &H2 '0x00000002 Ignores the dwDesiredAccess parameter. The duplicate handle has the same access as the source handle.
End Enum

Class Snapshot
    Public IntPtr As IntPtr
    Public CustomThreadID As Integer
    Public UserTime As TimeSpan
    Public KernelTime As TimeSpan
End Class