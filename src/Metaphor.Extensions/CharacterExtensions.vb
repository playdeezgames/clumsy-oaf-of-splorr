Imports System.Runtime.CompilerServices
Imports Metaphor.Persistence

Public Module CharacterExtensions
    <Extension>
    Public Function IsAvatar(character As ICharacter) As Boolean
        Return character.World.Avatar.EntityId = character.EntityId
    End Function
#Region "Counters"
    <Extension>
    Friend Function GetHealth(character As ICharacter) As Integer
        Return character.GetCounter(Counters.HEALTH)
    End Function
    <Extension>
    Friend Function GetStamina(character As ICharacter) As Integer
        Return character.GetCounter(Counters.STAMINA)
    End Function
    <Extension>
    Friend Function GetAttack(character As ICharacter) As Integer
        Return character.GetCounter(Counters.ATTACK)
    End Function
    <Extension>
    Friend Function GetDefend(character As ICharacter) As Integer
        Return character.GetCounter(Counters.DEFEND)
    End Function
#End Region
#Region "Show Status"
    <Extension>
    Public Sub ShowStatus(character As ICharacter)
        character.AddMessage($"Status:")
        character.AddMessage($"Health: {character.GetCounterStatistic(Counters.HEALTH)}")
    End Sub
#End Region
#Region "Look"
    <Extension>
    Public Sub Look(character As ICharacter)
        Dim location = character.Location
        If character.InCombat Then
            DescribeCombat(character)
        Else
            character.AddMessage($"{character.Name} is in {location.Name}.")
            DescribeFeatures(location)
        End If
    End Sub

    Private Sub DescribeCombat(character As ICharacter)
        character.AddMessage($"Health: {character.GetCounterStatistic(Counters.HEALTH)}")
        character.AddMessage($"Stamina: {character.GetCounterStatistic(Counters.STAMINA)}")
        Dim enemies = character.Location.GetEnemies()
        character.AddMessage($"{character.Name} is in combat with:")
        For Each enemy In enemies
            character.AddMessage($"- {enemy.Name}(Health: {enemy.GetCounterStatistic(Counters.HEALTH)}, Posture: {enemy.GetMetadata(Metadatas.POSTURE)})")
        Next
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
#Region "Checkpoint"
    <Extension>
    Public Sub SetCheckpoint(character As ICharacter, checkpoint As IFeature)
        character.SetYoke(Yokes.CHECKPOINT, checkpoint.EntityId)
    End Sub
    <Extension>
    Public Function GetCheckpoint(character As ICharacter) As IFeature
        Return character.World.GetFeature(character.GetYoke(Yokes.CHECKPOINT))
    End Function
    <Extension>
    Public Function IsCurrentCheckpoint(character As ICharacter, checkpoint As IFeature) As Boolean
        Return character.GetCheckpoint().EntityId = checkpoint.EntityId
    End Function
#End Region
#Region "Death/Respawn"
    <Extension>
    Public Function IsDead(character As ICharacter) As Boolean
        Return character.IsCounterMinimum(Counters.HEALTH)
    End Function
    <Extension>
    Friend Sub Die(character As ICharacter)
        character.AddMessage($"{character.Name} dies.")
        character.MinimizeCounter(Counters.HEALTH)
    End Sub
    <Extension>
    Public Sub Respawn(character As ICharacter)
        If Not character.IsAvatar Then
            Return
        End If
        'TODO: drop yer stuff in a gravestone?
        character.Location = character.GetCheckpoint().Location
        character.AddMessage($"{character.Name} respawns in {character.Location.Name}.")
        character.MaximumCounter(Counters.HEALTH)
    End Sub
#End Region
#Region "Combat"
    <Extension>
    Public Function InCombat(character As ICharacter) As Boolean
        Return character.IsAvatar AndAlso character.Location.Characters.Any(Function(x) x.HasTag(Tags.ENEMY) And Not x.IsDead)
    End Function
    <Extension>
    Public Function CanDodge(character As ICharacter) As Boolean
        Return character.IsAvatar
    End Function
    <Extension>
    Public Function CanParry(character As ICharacter) As Boolean
        Return character.IsAvatar
    End Function
    <Extension>
    Public Function CanFastAttack(character As ICharacter) As Boolean
        Return character.IsAvatar
    End Function
    <Extension>
    Public Function CanStrongAttack(character As ICharacter) As Boolean
        Return character.IsAvatar
    End Function
    <Extension>
    Public Sub DoRest(character As ICharacter)
        character.AddMessage($"{character.Name} rests.")
    End Sub
    <Extension>
    Public Sub DoDodge(character As ICharacter)
        character.AddMessage($"{character.Name} dodges.")
    End Sub
    <Extension>
    Public Sub DoParry(character As ICharacter)
        character.AddMessage($"{character.Name} parries.")
    End Sub
    <Extension>
    Public Sub DoFastAttack(attacker As ICharacter)
        attacker.AddMessage($"{attacker.Name} does fast attack.")
        Dim defender = attacker.Location.GetEnemies().First
        Dim attack = attacker.GetAttack()
        Dim defend = defender.GetDefend()
        Dim damage = Math.Max(attack - defend, 0)
        attacker.AddMessage($"{attacker.Name} does {damage} damage to {defender.Name}.")
        If damage > 0 Then
            defender.ChangeCounter(Counters.HEALTH, -damage)
            If defender.IsDead Then
                attacker.AddMessage($"{attacker.Name} kills {defender.Name}.")
                If Not defender.IsAvatar Then
                    defender.Remove()
                End If
            Else
                attacker.AddMessage($"{defender.Name} has {defender.GetCounterStatistic(Counters.HEALTH)} health.")
            End If
        End If
        attacker.DoCounterAttacks()
        If Not attacker.IsDead Then
            attacker.Look()
        End If
    End Sub
    <Extension>
    Private Sub DoCounterAttacks(defender As ICharacter)
        Dim attackers = defender.Location.GetEnemies()
        For Each attacker In attackers
            defender.AddMessage($"{attacker.Name} takes action.")
        Next
    End Sub
    <Extension>
    Public Sub DoStrongAttack(character As ICharacter)
        character.AddMessage($"{character.Name} does strong attack.")
    End Sub
#End Region
End Module
