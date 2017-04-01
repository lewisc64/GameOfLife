Imports System.Threading

Public Class Form1

    Private thread As New Thread(AddressOf mainLoop)
    Private display As VBGame.Display

    Public gridSize As Integer = 50

    Public Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        VBGame.XMLIO.knownTypes = {GetType(Cell), GetType(Saving.SaveContainer)}

        display = New VBGame.Display(Me, New Size(500, 500), "Conway's Game of Life", True)

        Dim surface As New VBGame.BitmapSurface(New Size(30, 30))
        surface.fill(VBGame.Colors.white)
        surface.drawRect(New Rectangle(0, 0, surface.width / 3, surface.height), Color.FromArgb(180, 180, 180))
        surface.drawRect(New Rectangle(surface.width / 3, surface.height * 2 / 3, surface.width / 3, surface.height / 3), Color.FromArgb(180, 180, 180))
        surface.drawRect(New Rectangle(surface.width * 2 / 3, surface.width / 3, surface.width / 3, surface.height / 3), Color.FromArgb(180, 180, 180))
        surface.drawText(surface.getRect(), "GOL", VBGame.Colors.black, New Font("Arial", 10, FontStyle.Bold))
        Me.Icon = Icon.FromHandle(surface.getImage().GetHicon())

        thread.Start()
    End Sub

    Public Sub mainLoop()
        While True
            simulationLoop()
        End While
    End Sub

    Public Sub simulationLoop()
        Dim Grid As New Grid(display, gridSize)
        Dim zoom As Double = 1
        Dim minZoom As Integer = 1
        Dim maxZoom As Integer = 5
        Dim zoomStep As Double = 1
        Dim targetZoom As Double = 1
        Dim zoomOnPoint As Point
        Dim shift As New Point(0, 0)

        display.fill(VBGame.Colors.white)

        'Grid.MakeAlive(Grid.cells(22, 44))
        'Grid.MakeAlive(Grid.cells(22, 45))
        'Grid.MakeAlive(Grid.cells(22, 46))
        'Grid.MakeAlive(Grid.cells(21, 46))
        'Grid.MakeAlive(Grid.cells(20, 45))

        'Grid.ForceAlive(Grid.cells(52, 44))
        'Grid.ForceAlive(Grid.cells(52, 45))
        'Grid.ForceAlive(Grid.cells(52, 46))
        'Grid.ForceAlive(Grid.cells(53, 46))
        'Grid.ForceAlive(Grid.cells(51, 45))

        'Grid.Randomize()

        Grid.Update()

        Dim cellDisplay As New VBGame.BitmapSurface(display.getRect().Size())
        cellDisplay.fill(VBGame.Colors.white)

        Dim nextFrame As Boolean
        Dim selection As Point

        Dim gridLines As Boolean = Grid.side >= 3

        Dim isPanning As Boolean = False
        Dim gripPoint As Point

        While True

            display.fill(VBGame.Colors.black)

            For Each e As KeyEventArgs In display.getKeyDownEvents()
                If e.KeyCode = Keys.Enter Then
                    nextFrame = True
                End If
            Next
            For Each e As KeyEventArgs In display.getKeyUpEvents()
                If e.KeyCode = Keys.R Then
                    targetZoom = 1
                    shift = New Point(0, 0)
                ElseIf e.KeyCode = Keys.S Then
                    Me.Invoke(Sub() Grid.Save())
                ElseIf e.KeyCode = Keys.L Then
                    Me.Invoke(Sub() Grid.Load())
                    'cellDisplay.fill(VBGame.Colors.white)
                    gridSize = Grid.width
                    gridLines = Grid.side >= 3
                ElseIf e.KeyCode = Keys.N Then
                    display.fill(VBGame.Colors.white)
                    gridSize = InputBox("Grid size?", "Game of Life", 50)
                    Exit While
                ElseIf e.KeyCode = Keys.G Then
                    gridLines = Not gridLines
                    Grid.MakeAllDirty()
                ElseIf e.KeyCode = Keys.E Then
                    Console.WriteLine(Saving.ToRLE(Grid))
                End If
            Next
            For Each e As VBGame.MouseEvent In display.getMouseEvents()
                If isPanning AndAlso e.action = VBGame.MouseEvent.actions.move Then
                    shift = New Point(gripPoint.X - e.location.X / zoom, gripPoint.Y - e.location.Y / zoom)
                End If
                If e.action = VBGame.MouseEvent.actions.down Then
                    If e.button = VBGame.MouseEvent.buttons.middle Then
                        isPanning = True
                        gripPoint = New Point(e.location.X / zoom + shift.X, e.location.Y / zoom + shift.Y)
                    End If
                ElseIf e.action = VBGame.MouseEvent.actions.up Then
                    If e.button = VBGame.MouseEvent.buttons.middle Then
                        isPanning = False
                    End If
                    If zoom = targetZoom Then
                        selection = New Point((((e.location.X) \ zoom + shift.X) \ Grid.side), (((e.location.Y) \ zoom + shift.Y) \ Grid.side))
                        If selection.X >= 0 AndAlso selection.X < Grid.cells.GetLength(0) Then
                            If selection.Y >= 0 AndAlso selection.Y < Grid.cells.GetLength(1) Then
                                If e.button = VBGame.MouseEvent.buttons.left Then
                                    If Not Grid.cells(selection.X, selection.Y).alive Then
                                        Grid.ForceAlive(Grid.cells(selection.X, selection.Y))
                                    End If
                                ElseIf e.button = VBGame.MouseEvent.buttons.right Then
                                    If Grid.cells(selection.X, selection.Y).alive Then
                                        Grid.ForceDead(Grid.cells(selection.X, selection.Y))
                                    End If
                                End If
                            End If
                        End If
                    End If
                ElseIf e.action = VBGame.MouseEvent.actions.scroll Then
                    If e.button = VBGame.MouseEvent.buttons.scrollUp Then
                        targetZoom = Math.Min(maxZoom, targetZoom + zoomStep)
                        zoomOnPoint = New Point(display.width / 2, display.height / 2)
                    ElseIf e.button = VBGame.MouseEvent.buttons.scrollDown Then
                        targetZoom = Math.Max(minZoom, targetZoom - zoomStep)
                        zoomOnPoint = New Point(display.width / 2, display.height / 2)
                    End If
                ElseIf e.button = VBGame.MouseEvent.buttons.middle Then
                    isPanning = False
                    Windows.Forms.Cursor.Show()
                End If
            Next

            If nextFrame Then
                Grid.Update()
                nextFrame = False
            End If

            If Math.Abs(zoom - targetZoom) < 0.001 Then
                zoom = targetZoom
            Else
                zoom += (targetZoom - zoom) / 10
                shift = New Point(shift.X + (shift.X - shift.X) / 2, shift.Y + (shift.Y - shift.Y) / 2)
            End If

            Grid.DrawDirty(cellDisplay)

            If gridLines Then
                For x = 0 To Grid.width * Grid.side - Grid.side Step Grid.side
                    cellDisplay.drawLine(New Point(x, 0), New Point(x, display.height), Color.LightGray)
                Next
                For y = 0 To Grid.height * Grid.side - Grid.side Step Grid.side
                    cellDisplay.drawLine(New Point(0, y), New Point(display.width, y), Color.LightGray)
                Next
            End If

            display.fill(VBGame.Images.getRegionOfImage(cellDisplay.getImage(), New Rectangle(shift.X, shift.Y, display.width / zoom, display.height / zoom)))
            'display.fill(VBGame.Images.GetRegionOfImage(cellDisplay.getImage(), New Rectangle(0, 0, 50, 50)))
            'Grid.DrawAll(display)

            display.clockTick(60)
            display.update()
        End While

    End Sub

End Class
