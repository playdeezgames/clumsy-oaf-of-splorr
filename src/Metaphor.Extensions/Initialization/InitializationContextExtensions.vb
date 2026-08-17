Imports System.Runtime.CompilerServices
Imports Metaphor.Extensions
Imports Metaphor.Persistence
Imports TGGD.Extensions

Friend Module InitializationContextExtensions
    <Extension>
    Friend Function InitializeBlueRoom(context As IInitializationContext) As Persistence.LocationInitializer
        Return Sub(room)
                   Dim checkpoint = room.CreateCheckpoint()
                   room.CreateN00b(context.ChosenName, context.InitializeN00b(checkpoint))
                   InitializeMaze(room)
               End Sub
    End Function
#Region "Maze"
    Private mazeDirections As New Dictionary(Of String, MazeDirection(Of String)) From
        {
            {Directions.NORTH, New MazeDirection(Of String)(Directions.SOUTH, 0, -1)},
            {Directions.EAST, New MazeDirection(Of String)(Directions.WEST, 1, 0)},
            {Directions.SOUTH, New MazeDirection(Of String)(Directions.NORTH, 0, 1)},
            {Directions.WEST, New MazeDirection(Of String)(Directions.EAST, -1, 0)}
        }
    Private Sub InitializeMaze(room As ILocation)
        Const MAZE_COLUMNS = 4
        Const MAZE_ROWS = 4
        Dim maze As New Maze(Of String)(MAZE_COLUMNS, MAZE_ROWS, mazeDirections)
        maze.Generate()
        Dim world = room.World
        For Each column In Enumerable.Range(0, MAZE_COLUMNS)
            For Each row In Enumerable.Range(0, MAZE_ROWS)
                world.CreateLocation(
                    LocationSubtypes.MAZE,
                    "Maze Room",
                    Sub(location)
                        location.SetCounter(Counters.MAZE_COLUMN, column)
                        location.SetCounter(Counters.MAZE_ROW, row)
                        world.AddToYokage(Yokages.MAZE_LOCATIONS, location.EntityId)
                    End Sub)
            Next
        Next
        Dim mazeLocations = world.GetYokage(Yokages.MAZE_LOCATIONS).Select(AddressOf world.GetLocation)
        For Each column In Enumerable.Range(0, MAZE_COLUMNS)
            For Each row In Enumerable.Range(0, MAZE_ROWS)
                Dim mazeCell = maze.GetCell(column, row)
                Dim mazeLocation = mazeLocations.Single(Function(x) x.GetCounter(Counters.MAZE_COLUMN) = column AndAlso x.GetCounter(Counters.MAZE_ROW) = row)
                For Each direction In mazeDirections.Keys
                    Dim door = mazeCell.GetDoor(direction)
                    If If(door?.Open, False) Then
                        Dim nextColumn = mazeDirections(direction).DeltaX + column
                        Dim nextRow = mazeDirections(direction).DeltaY + row
                        Dim nextLocation = mazeLocations.Single(Function(x) x.GetCounter(Counters.MAZE_COLUMN) = nextColumn AndAlso x.GetCounter(Counters.MAZE_ROW) = nextRow)
                        mazeLocation.CreateDoor(direction, nextLocation)
                    End If
                Next
            Next
        Next
        Dim entrance = RNG.FromEnumerable(mazeLocations)
        room.CreateDoor(Directions.OUT, entrance)
        entrance.CreateDoor(Directions.IN, room)
    End Sub
#End Region

    <Extension>
    Private Function InitializeN00b(context As IInitializationContext, checkpoint As IFeature) As CharacterInitializer
        Const MAXIMUM_HEALTH = 100
        Return Sub(character)
                   character.SetCheckpoint(checkpoint)
                   character.InitializeCounter(Counters.HEALTH, MAXIMUM_HEALTH, 0, MAXIMUM_HEALTH)
                   character.World.Avatar = character
               End Sub
    End Function
End Module
