import type { ReactNode } from 'react'
import { createContext, useContext, useState, useCallback } from 'react'

export interface ConsoleLine {
  text: string
  level: 'info' | 'warn' | 'error'
}

interface ConsoleContextType {
  lines: ConsoleLine[]
  append: (text: string, level?: 'info' | 'warn' | 'error') => void
  clear: () => void
  activeTab: string
  setActiveTab: (tab: string) => void
}

const ConsoleContext = createContext<ConsoleContextType | null>(null)

export function ConsoleProvider({ children }: { children: ReactNode }) {
  const [lines, setLines] = useState<ConsoleLine[]>([
    { text: '--- Td# Studio v0.1 ---', level: 'info' },
    { text: 'Ready', level: 'info' },
  ])
  const [activeTab, setActiveTab] = useState('output')

  const append = useCallback((text: string, level: 'info' | 'warn' | 'error' = 'info') => {
    setLines(prev => [...prev, { text, level }])
  }, [])

  const clear = useCallback(() => {
    setLines([])
  }, [])

  return (
    <ConsoleContext.Provider value={{ lines, append, clear, activeTab, setActiveTab }}>
      {children}
    </ConsoleContext.Provider>
  )
}

export function useConsole(): ConsoleContextType {
  const ctx = useContext(ConsoleContext)
  if (!ctx) throw new Error('useConsole must be used within ConsoleProvider')
  return ctx
}
