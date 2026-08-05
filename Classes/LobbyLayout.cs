using System;
using System.Collections.Generic;
using UnityEngine;

namespace IncreasedLobbyLimit.Classes;

public static class LobbyLayout
{
    private const int OriginalSlotCount = 6;

    public class SlotLayout(LobbySlotView view, Transform characterPivot, Transform sourcePivot, int row)
    {
        public readonly LobbySlotView View = view;
        public readonly Transform CharacterPivot = characterPivot;
        public readonly Vector3 BasePosition = sourcePivot.position;
        public readonly Quaternion BaseRotation = sourcePivot.rotation;
        public readonly Vector3 LookDirection = sourcePivot.forward;
        public readonly int Row = row;
    }

    private class LayoutState
    {
        public readonly List<SlotLayout> ExtraSlots = [];
    }

    private static readonly Dictionary<LobbyMenu, LayoutState> States = new();

    public static bool EnsureExpanded(LobbyMenu lobbyMenu)
    {
        if (lobbyMenu == null || lobbyMenu.slots == null || lobbyMenu.slots.Length >= Plugin.MaxPlayers)
            return false;

        if (States.ContainsKey(lobbyMenu))
            return false;

        if (lobbyMenu.slots.Length != OriginalSlotCount)
            return false;

        var originalSlots = lobbyMenu.slots;
        var expandedSlots = new LobbySlotView[Plugin.MaxPlayers];
        Array.Copy(originalSlots, expandedSlots, originalSlots.Length);

        var state = new LayoutState();
        var createdObjects = new List<UnityEngine.Object>();
        for (var index = OriginalSlotCount; index < Plugin.MaxPlayers; index++)
        {
            var source = originalSlots[index % OriginalSlotCount];
            if (source == null || source.characterPivot == null)
            {
                Cleanup(createdObjects);
                return false;
            }

            var cloneObject = UnityEngine.Object.Instantiate(source.gameObject, source.transform.parent, true);
            createdObjects.Add(cloneObject);
            cloneObject.name = $"{source.gameObject.name}_BiggerLobby_{index + 1}";

            var clone = cloneObject.GetComponent<LobbySlotView>();
            if (clone == null || clone.playerName == source.playerName)
            {
                Cleanup(createdObjects);
                return false;
            }

            clone.spawnedSkin = null;
            var characterPivot = EnsureIndependentPivot(source, clone, cloneObject, createdObjects);
            if (characterPivot == null)
            {
                Cleanup(createdObjects);
                return false;
            }

            var row = index / OriginalSlotCount;
            clone.extraNameHeight = source.extraNameHeight - row * 0.25f;
            cloneObject.SetActive(false);
            expandedSlots[index] = clone;
            state.ExtraSlots.Add(new SlotLayout(clone, characterPivot, source.characterPivot, row));
        }

        lobbyMenu.slots = expandedSlots;
        States[lobbyMenu] = state;

        return true;
    }

    public static void ApplyLayout(LobbyMenu lobbyMenu)
    {
        if (lobbyMenu == null || !States.TryGetValue(lobbyMenu, out var state))
            return;

        const float rowDepth = 1.5f;
        foreach (var slot in state.ExtraSlots)
        {
            if (slot.View == null || slot.CharacterPivot == null)
                continue;

            var lookDirection = slot.LookDirection;
            lookDirection.y = 0f;
            if (lookDirection.sqrMagnitude < 0.0001f)
            {
                lookDirection = Vector3.forward;
            }
            else
            {
                lookDirection.Normalize();
            }

            slot.CharacterPivot.SetPositionAndRotation(slot.BasePosition - lookDirection * (slot.Row * rowDepth),
                slot.BaseRotation);
        }
    }

    public static void RepositionSlots(LobbyMenu lobbyMenu)
    {
        if (lobbyMenu?.slots == null)
            return;

        foreach (var slot in lobbyMenu.slots)
        {
            slot.RepositionUIElements();
        }
    }

    private static Transform EnsureIndependentPivot(LobbySlotView source, LobbySlotView clone, GameObject cloneObject,
        List<UnityEngine.Object> createdObjects)
    {
        if (clone.characterPivot != source.characterPivot)
            return clone.characterPivot;

        var pivotObject = new GameObject($"{cloneObject.name}_CharacterPivot");
        createdObjects.Add(pivotObject);

        var pivot = pivotObject.transform;
        pivot.SetParent(source.characterPivot.parent, false);
        pivot.SetPositionAndRotation(source.characterPivot.position, source.characterPivot.rotation);
        pivot.localScale = source.characterPivot.localScale;
        clone.characterPivot = pivot;

        return pivot;
    }

    private static void Cleanup(List<UnityEngine.Object> createdObjects)
    {
        foreach (var createdObject in createdObjects)
        {
            if (createdObject == null)
                continue;

            UnityEngine.Object.Destroy(createdObject);
        }
    }
}
