Imports System.Runtime.CompilerServices
Imports Metaphor.Persistence

Public Module LocationExtensions
    <Extension>
    Friend Sub CreateN00b(location As ILocation, name As String, initializer As CharacterInitializer)
        location.CreateCharacter(CharacterSubtypes.N00B, name, initializer)
    End Sub
End Module
