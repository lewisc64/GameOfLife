Public Class Cell
    Public x As Integer
    Public y As Integer
    Public ix As Integer
    Public iy As Integer
    Public side As Integer

    Public alive As Boolean

    Public Sub New(ix As Integer, iy As Integer, side As Integer)
        Me.ix = ix
        Me.iy = iy
        Me.side = side
        x = ix * side
        y = iy * side
        alive = False
    End Sub

    Public Shared Operator =(Cell1 As Cell, Cell2 As Cell)
        Return Cell1.ix = Cell2.ix AndAlso Cell1.iy = Cell2.iy
    End Operator

    Public Shared Operator <>(Cell1 As Cell, Cell2 As Cell)
        Return Not Cell1 = Cell2
    End Operator

    Public Sub Handle(Grid As Grid)
        Dim neighbours As List(Of Cell) = Grid.GetNeighbours(Me, Grid)
        Dim aliveCount As Integer = 0
        For Each Cell As Cell In neighbours
            If Cell.alive Then
                aliveCount += 1
            End If
        Next
        If aliveCount < 2 OrElse aliveCount > 3 Then
            Grid.MakeDead(Me)
        End If
        Dim neighbourAliveCount As Integer
        For Each Cell As Cell In Grid.GetNeighbours(Me, Grid)
            If Not Cell.alive Then
                neighbourAliveCount = 0
                For Each Cell2 As Cell In Grid.GetNeighbours(Cell, Grid)
                    If Cell2.alive Then
                        neighbourAliveCount += 1
                    End If
                Next
                If neighbourAliveCount = 3 Then
                    Grid.MakeAlive(Cell)
                End If
            End If
        Next
    End Sub

    Public Function GetRect() As Rectangle
        Return New Rectangle(x, y, side, side)
    End Function

    Public Sub Draw(display As VBGame.DrawBase)
        If alive Then
            display.drawRect(GetRect(), VBGame.Colors.black)
        Else
            display.drawRect(GetRect(), VBGame.Colors.white)
        End If
    End Sub

End Class
