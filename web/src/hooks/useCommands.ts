import { useState, useCallback } from 'react'

interface Command {
  do: () => void
  undo: () => void
  label: string
}

export function useCommands() {
  const [undoStack, setUndoStack] = useState<Command[]>([])
  const [redoStack, setRedoStack] = useState<Command[]>([])

  const execute = useCallback((cmd: Command) => {
    cmd.do()
    setUndoStack(prev => [...prev, cmd])
    setRedoStack([])
  }, [])

  const undo = useCallback(() => {
    setUndoStack(prev => {
      if (prev.length === 0) return prev
      const cmd = prev[prev.length - 1]
      cmd.undo()
      setRedoStack(r => [...r, cmd])
      return prev.slice(0, -1)
    })
  }, [])

  const redo = useCallback(() => {
    setRedoStack(prev => {
      if (prev.length === 0) return prev
      const cmd = prev[prev.length - 1]
      cmd.do()
      setUndoStack(u => [...u, cmd])
      return prev.slice(0, -1)
    })
  }, [])

  const canUndo = undoStack.length > 0
  const canRedo = redoStack.length > 0

  return { execute, undo, redo, canUndo, canRedo }
}
