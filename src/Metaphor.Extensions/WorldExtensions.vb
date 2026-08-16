Imports System.Runtime.CompilerServices
Imports Metaphor.Persistence

Public Module WorldExtensions
    <Extension>
    Private Sub CreateBlueRoom(world As IWorld, context As IInitializationContext)
        world.CreateLocation(LocationSubtypes.BLUE_ROOM, "The Blue Room", context.InitializeBlueRoom())
    End Sub
    <Extension>
    Public Sub Initialize(world As IWorld, context As IInitializationContext)
        world.Clear()
        world.CreateBlueRoom(context)
        world.AddMessage("Welcome to Clumsy Oaf of SPLORR!!")
        world.Avatar.Look()
    End Sub
End Module
