Imports System.Runtime.CompilerServices
Imports Metaphor.Persistence
Imports TGGD.Extensions

Public Module CharacterExtensions
    <Extension>
    Public Function IsAvatar(character As ICharacter) As Boolean
        Return character.World.Avatar.EntityId = character.EntityId
    End Function
#Region "Counters"
    <Extension>
    Friend Sub SpendStamina(character As ICharacter, stamina As Integer)
        If Not character.IsAvatar Then
            Return
        End If
        character.AddMessage($"{character.Name} loses {stamina} stamina.")
        character.ChangeCounter(Counters.STAMINA, -stamina)
        character.AddMessage($"{character.Name} now has {character.GetCounterStatistic(Counters.STAMINA)} stamina.")
    End Sub
    <Extension>
    Friend Sub RecoverStamina(character As ICharacter, stamina As Integer)
        If Not character.IsAvatar Then
            Return
        End If
        character.AddMessage($"{character.Name} gains {stamina} stamina.")
        character.ChangeCounter(Counters.STAMINA, stamina)
        character.AddMessage($"{character.Name} now has {character.GetCounterStatistic(Counters.STAMINA)} stamina.")
    End Sub
    <Extension>
    Friend Function GetHealth(character As ICharacter) As Integer
        Return character.GetCounter(Counters.HEALTH)
    End Function
    <Extension>
    Friend Sub DoDamage(character As ICharacter, damage As Integer)
        character.ChangeCounter(Counters.HEALTH, -damage)
    End Sub
    <Extension>
    Friend Function GetStamina(character As ICharacter) As Integer
        Return character.GetCounter(Counters.STAMINA)
    End Function
    <Extension>
    Friend Function GetAttack(character As ICharacter, Optional effectiveness As Double = 1.0) As Integer
        Return CInt(character.GetCounter(Counters.ATTACK) * effectiveness)
    End Function
    <Extension>
    Friend Function GetDefend(character As ICharacter) As Integer
        Return character.GetCounter(Counters.DEFEND)
    End Function
    <Extension>
    Friend Function GetPosture(character As ICharacter) As String
        Return character.GetMetadata(Metadatas.POSTURE)
    End Function
    <Extension>
    Friend Sub SetPosture(character As ICharacter, posture As String)
        character.SetMetadata(Metadatas.POSTURE, posture)
    End Sub
    <Extension>
    Friend Function GetDodgeCost(character As ICharacter) As Integer
        If Not character.IsAvatar Then Return 0
        Return Math.Max(1, character.GetCounterMaximum(Counters.STAMINA) \ 10)
    End Function
    <Extension>
    Friend Function GetRestRecovery(character As ICharacter) As Integer
        If Not character.IsAvatar Then Return 0
        Return Math.Max(1, character.GetCounterMaximum(Counters.STAMINA) \ 2)
    End Function
    <Extension>
    Friend Function GetParryCost(character As ICharacter) As Integer
        If Not character.IsAvatar Then Return 0
        Return Math.Max(1, character.GetCounterMaximum(Counters.STAMINA) \ 5)
    End Function
    <Extension>
    Friend Function GetFastAttackCost(character As ICharacter) As Integer
        If Not character.IsAvatar Then Return 0
        Return Math.Max(1, character.GetCounterMaximum(Counters.STAMINA) \ 3)
    End Function
    <Extension>
    Friend Function GetStrongAttackCost(character As ICharacter) As Integer
        If Not character.IsAvatar Then Return 0
        Return Math.Max(1, character.GetCounterMaximum(Counters.STAMINA) * 2 \ 3)
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
        Return character.IsAvatar AndAlso character.GetStamina() > character.GetDodgeCost()
    End Function
    <Extension>
    Public Function CanParry(character As ICharacter) As Boolean
        Return character.IsAvatar AndAlso character.GetStamina() > character.GetParryCost()
    End Function
    <Extension>
    Public Function CanFastAttack(character As ICharacter) As Boolean
        Return character.IsAvatar AndAlso character.GetStamina() > character.GetFastAttackCost()
    End Function
    <Extension>
    Public Function CanStrongAttack(character As ICharacter) As Boolean
        Return character.IsAvatar AndAlso character.GetStamina() > character.GetStrongAttackCost()
    End Function
    <Extension>
    Public Sub DoRest(character As ICharacter)
        character.SetPosture(Postures.REST)
        character.RecoverStamina(character.GetRestRecovery())
        character.AddMessage($"{character.Name} rests.")
        character.EndCombatTurn()
    End Sub
    <Extension>
    Public Sub DoDodge(character As ICharacter)
        character.SetPosture(Postures.DODGE)
        character.SpendStamina(character.GetDodgeCost())
        character.AddMessage($"{character.Name} dodges.")
        character.EndCombatTurn()
    End Sub
    <Extension>
    Public Sub DoParry(character As ICharacter)
        character.SetPosture(Postures.PARRY)
        character.SpendStamina(character.GetParryCost())
        character.AddMessage($"{character.Name} parries.")
        character.EndCombatTurn()
    End Sub
    <Extension>
    Public Sub DoFastAttack(attacker As ICharacter)
        attacker.SetPosture(Postures.FAST_ATTACK)
        attacker.SpendStamina(attacker.GetFastAttackCost())
        attacker.AddMessage($"{attacker.Name} does fast attack.")
        attacker.ResolveAttack()
        attacker.EndCombatTurn()
    End Sub
    Private ReadOnly attackEffectiveness As New Dictionary(Of String, Dictionary(Of String, Double)) From
        {
            {
                Postures.FAST_ATTACK,
                New Dictionary(Of String, Double) From
                {
                    {Postures.PARRY, 0.5},
                    {Postures.DODGE, 0.0},
                    {Postures.REST, 1.5}
                }
            },
            {
                Postures.STRONG_ATTACK,
                New Dictionary(Of String, Double) From
                {
                    {Postures.PARRY, 0.0},
                    {Postures.DODGE, 0.5},
                    {Postures.REST, 1.5}
                }
            }
        }
    Private Function GetEffectiveness(attackPosture As String, defendPosture As String) As Double
        Dim table As Dictionary(Of String, Double) = Nothing
        If attackEffectiveness.TryGetValue(attackPosture, table) Then
            Dim effectiveness As Double = 0.0
            If table.TryGetValue(defendPosture, effectiveness) Then
                Return effectiveness
            End If
        End If
        Return 1.0
    End Function
    <Extension>
    Private Sub ResolveAttack(attacker As ICharacter)
        Dim defender = If(attacker.IsAvatar, attacker.Location.GetEnemies().First, attacker.World.Avatar)
        Dim attack = attacker.GetAttack(GetEffectiveness(attacker.GetPosture(), defender.GetPosture()))
        Dim defend = defender.GetDefend()
        Dim damage = Math.Max(attack - defend, 0)
        attacker.AddMessage($"{attacker.Name} does {damage} damage to {defender.Name}.")
        If damage > 0 Then
            defender.DoDamage(damage)
            If defender.IsDead Then
                attacker.AddMessage($"{attacker.Name} kills {defender.Name}.")
                If Not defender.IsAvatar Then
                    defender.Remove()
                End If
            Else
                attacker.AddMessage($"{defender.Name} has {defender.GetCounterStatistic(Counters.HEALTH)} health.")
            End If
        End If
    End Sub

    <Extension>
    Public Sub DoStrongAttack(character As ICharacter)
        character.SetPosture(Postures.STRONG_ATTACK)
        character.SpendStamina(character.GetStrongAttackCost())
        character.AddMessage($"{character.Name} does strong attack.")
        character.ResolveAttack()
        character.EndCombatTurn()
    End Sub
    Private postureGenerator As New Dictionary(Of String, Integer) From
        {
            {Postures.DODGE, 1},
            {Postures.FAST_ATTACK, 1},
            {Postures.PARRY, 1},
            {Postures.REST, 1},
            {Postures.STRONG_ATTACK, 1}
        }
    <Extension>
    Friend Sub GeneratePosture(character As ICharacter)
        character.SetPosture(RNG.FromGenerator(postureGenerator))
    End Sub
    <Extension>
    Private Sub EndCombatTurn(attacker As ICharacter)
        If attacker.IsAvatar Then
            attacker.DoCounterAttacks()
            If Not attacker.IsDead Then
                attacker.Look()
            End If
        Else
            attacker.GeneratePosture()
        End If
    End Sub
#Region "Counter Attacks"
    Private Delegate Sub CounterAttackDelegate(character As ICharacter)
    Private ReadOnly counterAttacks As New Dictionary(Of String, CounterAttackDelegate) From
        {
            {Postures.DODGE, AddressOf DoDodge},
            {Postures.FAST_ATTACK, AddressOf DoFastAttack},
            {Postures.PARRY, AddressOf DoParry},
            {Postures.REST, AddressOf DoRest},
            {Postures.STRONG_ATTACK, AddressOf DoStrongAttack}
        }
    <Extension>
    Private Sub DoCounterAttacks(defender As ICharacter)
        Dim attackers = defender.Location.GetEnemies()
        For Each attacker In attackers
            counterAttacks(attacker.GetPosture()).Invoke(attacker)
        Next
    End Sub
#End Region
#End Region
End Module
