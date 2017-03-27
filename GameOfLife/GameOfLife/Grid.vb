Public Class Grid

    Public cells(,) As Cell

    Public side As Integer

    Private alive As New List(Of Cell)
    Private updateList As New List(Of Cell)
    Private dirty As New List(Of Cell)

    Private random As New Random()

    Public Sub New(display As VBGame.DrawBase, size As Integer)
        side = display.width / size
        Dim temp(display.width / side - 1, display.height / side - 1) As Cell
        cells = temp
        For x = 0 To cells.GetLength(0) - 1
            For y = 0 To cells.GetLength(1) - 1
                cells(x, y) = New Cell(x, y, side)
            Next
        Next
    End Sub

    Public Sub Randomize()
        For x = 0 To cells.GetLength(0) - 1
            For y = 0 To cells.GetLength(1) - 1
                If random.Next(1, 3) = 1 Then
                    ForceAlive(cells(x, y))
                End If
            Next
        Next
    End Sub

    Public Sub MakeAlive(Cell As Cell)
        updateList.Add(Cell)
    End Sub

    Public Sub ForceAlive(Cell As Cell)
        alive.Add(Cell)
        Cell.alive = True
        dirty.Add(Cell)
    End Sub

    Public Sub MakeDead(Cell As Cell)
        updateList.Add(Cell)
    End Sub

    Public Sub ForceDead(Cell As Cell)
        alive.Remove(Cell)
        Cell.alive = False
        dirty.Add(Cell)
    End Sub

    Public Sub Update()
        'Dim timer As Stopwatch = Stopwatch.StartNew()
        For Each Cell As Cell In alive.ToList()
            Cell.Handle(Me)
        Next
        'timer.Stop()
        'Debug.WriteLine("Cell logic time: " & timer.ElapsedMilliseconds)
        'timer.Restart()
        For Each Cell As Cell In updateList
            Cell.alive = Not Cell.alive
            If Cell.alive Then
                alive.Add(Cell)
            Else
                alive.Remove(Cell)
            End If
            dirty.Add(Cell)
        Next
        updateList.Clear()
        'timer.Stop()
        'Debug.WriteLine("Update time: " & timer.ElapsedMilliseconds)
        'timer.Restart()
    End Sub

    Public Sub DrawDirty(display As VBGame.DrawBase)
        For Each Cell As Cell In dirty
            Cell.Draw(display)
        Next
        dirty.Clear()
    End Sub

    Public Sub DrawAll(display As VBGame.DrawBase)
        For x As Integer = 0 To cells.GetLength(0) - 1
            For y As Integer = 0 To cells.GetLength(1) - 1
                cells(x, y).Draw(display)
            Next
        Next
    End Sub

    Public Shared Function GetNeighbours(Cell As Cell, Grid As Grid)
        Dim neighbours As New List(Of Cell)
        For x As Integer = Cell.ix - 1 To Cell.ix + 1
            For y As Integer = Cell.iy - 1 To Cell.iy + 1
                If x >= 0 AndAlso y >= 0 AndAlso x < Grid.cells.GetLength(0) AndAlso y < Grid.cells.GetLength(1) AndAlso Not (x = Cell.ix And y = Cell.iy) Then
                    neighbours.Add(Grid.cells(x, y))
                End If
            Next
        Next
        Return neighbours
    End Function

End Class