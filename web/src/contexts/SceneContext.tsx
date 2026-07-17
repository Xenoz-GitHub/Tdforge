import type { ReactNode } from 'react'
import { createContext, useContext, useState } from 'react'

export interface SceneObject {
  id: string
  name: string
  type: string
  x: number
  y: number
  width: number
  height: number
  rotation: number
  scaleX: number
  scaleY: number
  visible: boolean
  locked: boolean
  sprite: string
  color: string
  opacity: number
  children: SceneObject[]
}

interface SceneContextType {
  objects: SceneObject[]
  selectedId: string | null
  selectObject: (id: string | null) => void
  addObject: (obj: SceneObject) => void
  removeObject: (id: string) => void
  updateObject: (id: string, props: Partial<SceneObject>) => void
  duplicateObject: (id: string) => void
}

const SceneContext = createContext<SceneContextType | null>(null)

export function SceneProvider({ children }: { children: ReactNode }) {
  const [objects, setObjects] = useState<SceneObject[]>([])
  const [selectedId, setSelectedId] = useState<string | null>(null)

  const selectObject = (id: string | null) => setSelectedId(id)

  const addObject = (obj: SceneObject) => setObjects(prev => [...prev, obj])

  const removeObject = (id: string) => {
    setObjects(prev => prev.filter(o => o.id !== id))
    if (selectedId === id) setSelectedId(null)
  }

  const updateObject = (id: string, props: Partial<SceneObject>) => {
    setObjects(prev => prev.map(o => (o.id === id ? { ...o, ...props } : o)))
  }

  const duplicateObject = (id: string) => {
    const obj = objects.find(o => o.id === id)
    if (!obj) return
    const dup: SceneObject = { ...obj, id: crypto.randomUUID(), name: obj.name + '_copy', x: obj.x + 20, y: obj.y + 20 }
    addObject(dup)
    setSelectedId(dup.id)
  }

  return (
    <SceneContext.Provider value={{ objects, selectedId, selectObject, addObject, removeObject, updateObject, duplicateObject }}>
      {children}
    </SceneContext.Provider>
  )
}

export function useScene(): SceneContextType {
  const ctx = useContext(SceneContext)
  if (!ctx) throw new Error('useScene must be used within SceneProvider')
  return ctx
}
