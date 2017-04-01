Imports System.Text.RegularExpressions
Imports System.IO

Public Class Grid

    Public cells(,) As Cell

    Public side As Integer

    Public alive As New List(Of Cell)
    Private updateList As New List(Of Cell)
    Private dirty As New List(Of Cell)

    Private random As New Random()

    Public width As Integer
    Public height As Integer

    Public Sub New()
    End Sub

    Public Sub New(display As VBGame.DrawBase, size As Integer)
        side = display.width / size
        Dim temp(display.width / side - 1, display.height / side - 1) As Cell
        cells = temp
        width = cells.GetLength(0)
        height = cells.GetLength(1)
        For x = 0 To width - 1
            For y = 0 To height - 1
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
        For x As Integer = 0 To width - 1
            For y As Integer = 0 To height - 1
                cells(x, y).Draw(display)
            Next
        Next
    End Sub

    Public Shared Function GetNeighbours(Cell As Cell, Grid As Grid)
        Dim neighbours As New List(Of Cell)
        For x As Integer = Cell.ix - 1 To Cell.ix + 1
            For y As Integer = Cell.iy - 1 To Cell.iy + 1
                If x >= 0 AndAlso y >= 0 AndAlso x < Grid.width AndAlso y < Grid.height AndAlso Not (x = Cell.ix And y = Cell.iy) Then
                    neighbours.Add(Grid.cells(x, y))
                End If
            Next
        Next
        Return neighbours
    End Function

    Public Sub RebuildGrid()
        Dim temp(width - 1, height - 1) As Cell
        cells = temp
        For x As Integer = 0 To width - 1
            For y As Integer = 0 To height - 1
                cells(x, y) = New Cell(x, y, side)
            Next
        Next
        For Each Cell As Cell In alive
            cells(Cell.ix, Cell.iy) = Cell
        Next
    End Sub

    Private Shared dialogFilter As String = "GameOfLife File|*.gol|Run Length Encoded File|*.rle|Portable Network Graphics|*.png"

    Public Sub Save()
        Dim dialog As New SaveFileDialog
        dialog.Filter = dialogFilter
        If dialog.ShowDialog() = DialogResult.OK Then
            If dialog.FileName.Contains(".gol") Then
                Console.WriteLine(dialog.FilterIndex)
                VBGame.XMLIO.Write(dialog.FileName & If(dialog.FileName.Contains(".gol"), "", ".gol"), New Saving.SaveContainer(Me))
            ElseIf dialog.FileName.Contains(".rle") Then
                Dim writer As New StreamWriter(dialog.FileName)
                writer.Write(Saving.ToRLE(Me))
                writer.Close()
            ElseIf dialog.FileName.Contains(".png") Then
                VBGame.Images.save(Saving.ToImage(Me), dialog.FileName)
            End If
        End If
    End Sub

    Public Sub Load()
        Dim dialog As New OpenFileDialog
        dialog.Filter = dialogFilter
        If dialog.ShowDialog() = DialogResult.OK Then
            Dim grid As New Grid
            If dialog.FileName.Contains(".gol") Then
                Dim save As New Saving.SaveContainer
                VBGame.XMLIO.Read(dialog.FileName, save)
                grid = save.GetGrid()
            ElseIf dialog.FileName.Contains(".rle") Then
                Dim reader As New StreamReader(dialog.FileName)
                grid = Saving.FromRLE(reader.ReadToEnd())
                grid.RebuildGrid()
            ElseIf dialog.FileName.Contains(".png") Then
                grid = Saving.FromImage(VBGame.Images.load(dialog.FileName))
                grid.RebuildGrid()
            End If
            cells = grid.cells.Clone()
            alive = grid.alive.ToList()
            width = grid.width
            height = grid.height
            side = grid.side
            MakeAllDirty()
            updateList.Clear()
        End If
    End Sub

    Public Sub MakeAllDirty()
        For x As Integer = 0 To width - 1
            For y As Integer = 0 To height - 1
                dirty.Add(cells(x, y))
            Next
        Next
    End Sub

End Class

Public Class Saving

    Private Shared Function GetChar(Cell As Cell) As String
        Return If(Cell.alive, "o", "b")
    End Function

    Public Shared Function ToImage(grid As Grid) As Bitmap
        Dim image As New Bitmap(grid.width, grid.height)
        Dim g As Graphics = Graphics.FromImage(image)
        g.FillRectangle(Brushes.White, New Rectangle(0, 0, image.Width, image.Height))
        For Each Cell As Cell In grid.alive
            g.FillRectangle(Brushes.Black, New Rectangle(Cell.ix, Cell.iy, 1, 1))
        Next
        Return image
    End Function

    Public Shared Function FromImage(image As Bitmap)
        Dim grid As New Grid
        grid.side = 1
        grid.width = image.Width
        grid.height = image.Height
        Dim cell As Cell
        For y As Integer = 0 To image.Height - 1
            For x As Integer = 0 To image.Width - 1
                If image.GetPixel(x, y).GetBrightness < 0.5 Then
                    cell = New Cell(x, y, grid.side)
                    cell.alive = True
                    grid.alive.Add(cell)
                End If
            Next
        Next
        Return grid
    End Function

    Public Shared Function ToRLE(grid As Grid) As String
        Dim file As String = "#C Grid.side = " & grid.side & vbCrLf & "x = " & grid.width & ", y = " & grid.height & vbCrLf
        Dim encoding As String = ""
        For y As Integer = 0 To grid.height - 1
            For x As Integer = 0 To grid.width - 1
                encoding &= GetChar(grid.cells(x, y))
            Next
            If y < grid.height - 1 Then
                encoding &= "$"
            End If
        Next
        encoding &= "!"
        Dim current As String = ""
        Dim add As Boolean
        Dim i As Integer = 0
        While i < encoding.Length
            If current = "" Then
                current = encoding(i)
            End If
            add = False
            If encoding(i) = "o" OrElse encoding(i) = "b" Then
                If current(0) <> encoding(i) Then
                    add = True
                Else
                    current &= encoding(i)
                End If
            Else
                add = True
            End If
            If add Then
                If current.Length = 1 Then
                    file &= current
                Else
                    file &= If(current.Length - 1 = 1, "", current.Length - 1) & current(0)
                    i -= 1
                End If
                current = ""
            End If
            i += 1
        End While
        Return file
    End Function

    Public Shared Function FromRLE(file As String) As Grid
        If Not file.Contains(vbCrLf) Then
            If file.Contains(vbCr) Then
                file.Replace(vbCr, vbCrLf)
            ElseIf file.Contains(vbLf) Then
                file.Replace(vbLf, vbCrLf)
            End If
        End If
        Dim grid As New Grid
        grid.side = 10
        Dim sections As List(Of String) = file.Split({CChar(vbCrLf)}).ToList()
        For Each section As String In sections.ToList()
            If section.Contains("#") Then
                If section.Contains("#C Grid.side = ") Then
                    grid.side = CInt(section.Split({CChar("=")})(1).Trim())
                End If
                sections.Remove(section) 'I don't need all this information (probably).
            End If
        Next
        Dim header As String = sections(0)
        Dim data As String = sections(1)
        Dim matches As MatchCollection = Regex.Matches(header, "[0-9]+")
        grid.width = matches.Item(0).Value
        grid.height = matches.Item(1).Value
        matches = Regex.Matches(data, "([0-9]*[bo]|\$)")
        Dim x As Integer = 0
        Dim y As Integer = 0
        Dim split As MatchCollection
        Dim cell As Cell
        For Each Match As Match In matches
            If Match.Value.Length = 1 Then
                If Match.Value = "$" Then
                    x = 0
                    y += 1
                Else
                    If Match.Value = "o" Then
                        cell = New Cell(x, y, grid.side)
                        cell.alive = True
                        grid.alive.Add(cell)
                    End If
                    x += 1
                End If
                Continue For
            End If
            split = Regex.Matches(Match.Value, "([0-9]+|[bo])")
            For i As Integer = 1 To split.Item(0).Value
                If split.Item(1).Value = "o" Then
                    cell = New Cell(x, y, grid.side)
                    cell.alive = True
                    grid.alive.Add(cell)
                End If
                x += 1
            Next
        Next
        Return grid
    End Function

    Public Class SaveContainer

        Public side As Integer
        Public alive As New List(Of Cell)
        Public width As Integer
        Public height As Integer

        Public Sub New()
        End Sub

        Public Sub New(grid As Grid)
            side = grid.side
            alive = grid.alive.ToList()
            width = grid.width
            height = grid.height
        End Sub

        Public Function GetGrid() As Grid
            Dim grid As New Grid()
            grid.side = side
            grid.width = width
            grid.height = height
            grid.alive = alive.ToList()
            grid.RebuildGrid()
            Return grid
        End Function

    End Class
End Class