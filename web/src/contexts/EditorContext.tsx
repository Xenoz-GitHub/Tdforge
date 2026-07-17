import type { ReactNode } from 'react'
import { createContext, useContext, useState } from 'react'

interface EditorContextType {
  code: string
  setCode: (code: string) => void
}

const EditorContext = createContext<EditorContextType | null>(null)

export function EditorProvider({ children }: { children: ReactNode }) {
  const [code, setCode] = useState('// Welcome to Td# Studio\n\nfunction start()\n    say("Hello, Td#")\nend\n\nfunction tick(dt)\n    clear(#1a1a2e)\n    circle(320, 240, 50, Color(255, 100, 100))\nend')
  return (
    <EditorContext.Provider value={{ code, setCode }}>
      {children}
    </EditorContext.Provider>
  )
}

export function useEditor(): EditorContextType {
  const ctx = useContext(EditorContext)
  if (!ctx) throw new Error('useEditor must be used within EditorProvider')
  return ctx
}
