Imports System.Runtime.InteropServices
Imports System.Threading
Imports DiagnosticsUtils
Module Module1

    Private list As New List(Of IntPtr)

    Sub Main()
        Dim timeTrack As TimeThreadCheckService = New TimeThreadCheckService()

        For i As Integer = 0 To 2
            Dim deleg As ParameterizedThreadStart = New ParameterizedThreadStart(
                Sub(number As Object)
                    timeTrack.SetForCheckCurrentThread(i)
                    TestSub()
                End Sub)
            Dim th As New Thread(deleg)
            th.Start(i)

        Next

        Console.ReadKey()
    End Sub

    Private Sub TestSub()
        Dim list As New List(Of Integer)
        For i As Integer = 0 To 10
            Dim a = i * 3 / 2
            list.Add(a)
        Next
        list.Clear()
        For i As Integer = 0 To 10
            Dim a = i * 3 / 2
            list.Add(a)
        Next
        list.OrderBy(Function(x) x)
        Using client As New System.Net.WebClient
            Dim html = client.DownloadString("https://www.amazon.com/")
            System.Threading.Thread.Sleep(5000)
        End Using
    End Sub

    <DllImport("Kernel32.dll", EntryPoint:="GetCurrentThreadId", SetLastError:=True, CharSet:=CharSet.Auto)>
    Private Function GetCurrentWin32ThreadId() As Integer

    End Function
End Module
