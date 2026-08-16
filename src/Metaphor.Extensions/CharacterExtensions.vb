Imports System.Runtime.CompilerServices
Imports Metaphor.Persistence

Public Module CharacterExtensions
#Region "Show Status"
    <Extension>
    Public Sub ShowStatus(character As ICharacter)
        character.AddMessage($"Status:")
        character.AddMessage($"Health: {character.GetCounterStatistic(Counters.HEALTH)}")
        character.AddMessage($"Energy: {character.GetCounterStatistic(Counters.ENERGY)}")
        character.AddMessage($"Jools: {character.GetDimension(Dimensions.JOOLS):F2}")
    End Sub
#End Region
#Region "Look"
    <Extension>
    Public Sub Look(character As ICharacter)
        If character.IsDead Then
            character.AddMessage($"{character.Name} is dead.")
            Return
        End If
        Dim location = character.Location
        character.AddMessage($"{character.Name} is in {location.Name}.")
        DescribeFeatures(location)
    End Sub
    Private Sub DescribeFeatures(location As ILocation)
        If Not location.HasFeatures Then
            Return
        End If
        location.AddMessage($"Features:")
        For Each feature In location.Features
            location.AddMessage($"- {feature.Name}")
        Next
    End Sub
#End Region
#Region "Biology"
    <Extension>
    Public Function IsDead(character As ICharacter) As Boolean
        Return character.IsCounterMinimum(Counters.HEALTH)
    End Function
    <Extension>
    Public Sub DoBiology(character As ICharacter, Optional amount As Integer = 1)
        If character.IsDead() OrElse amount <= 0 Then
            Return
        End If
        Dim energy = Math.Min(amount, character.GetCounter(Counters.ENERGY))
        amount -= energy
        If energy > 0 Then
            character.AddMessage($"{character.Name} loses {energy} energy.")
            character.ChangeCounter(Counters.ENERGY, -energy)
            character.AddMessage($"{character.Name} now has {character.GetCounterStatistic(Counters.ENERGY)} energy.")
        End If
        Dim health = Math.Min(amount, character.GetCounter(Counters.HEALTH))
        amount -= health
        If health > 0 Then
            character.AddMessage($"{character.Name} loses {health} health.")
            character.ChangeCounter(Counters.HEALTH, -health)
            character.AddMessage($"{character.Name} now has {character.GetCounterStatistic(Counters.HEALTH)} health.")
            If character.IsCounterMinimum(Counters.HEALTH) Then
                character.AddMessage($"{character.Name} dies.")
            End If
        End If
    End Sub
#End Region
End Module
