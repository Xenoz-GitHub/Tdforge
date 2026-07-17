import { useEffect, useRef } from 'react'
import { useConsole } from '@/contexts/ConsoleContext'

const tabs = [
  { id: 'output', label: 'OUTPUT' },
  { id: 'code', label: 'CODE' },
  { id: 'find', label: 'FIND RESULTS' },
  { id: 'signals', label: 'SIGNALS' },
]

export function ConsolePanel() {
  const { lines, activeTab, setActiveTab } = useConsole()
  const scrollRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    if (scrollRef.current) {
      scrollRef.current.scrollTop = scrollRef.current.scrollHeight
    }
  }, [lines])

  if (activeTab === 'code') {
    return (
      <div style={styles.container}>
        <div style={styles.tabBar}>
          {tabs.map(tab => (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id)}
              style={{
                ...styles.tab,
                ...(activeTab === tab.id ? styles.tabActive : {}),
              }}
            >
              {tab.label}
            </button>
          ))}
        </div>
        <div style={styles.codeArea}>
          {/* Code editor lives in the center panel, not here */}
          <span style={{ color: '#443055' }}>Code panel (use Monaco editor in center)</span>
        </div>
      </div>
    )
  }

  return (
    <div style={styles.container}>
      <div style={styles.tabBar}>
        {tabs.map(tab => (
          <button
            key={tab.id}
            onClick={() => setActiveTab(tab.id)}
            style={{
              ...styles.tab,
              ...(activeTab === tab.id ? styles.tabActive : {}),
            }}
          >
            {tab.label}
          </button>
        ))}
      </div>
      <div ref={scrollRef} style={styles.output}>
        {lines.map((line, i) => (
          <div
            key={i}
            style={{
              ...styles.line,
              ...(line.level === 'error' ? styles.lineError : {}),
              ...(line.level === 'warn' ? styles.lineWarn : {}),
            }}
          >
            {line.text}
          </div>
        ))}
      </div>
    </div>
  )
}

const styles: Record<string, React.CSSProperties> = {
  container: {
    display: 'flex',
    flexDirection: 'column',
    height: '100%',
    background: '#1a1020',
  },
  tabBar: {
    display: 'flex',
    gap: 2,
    background: '#2a1838',
    padding: '2px 4px',
  },
  tab: {
    background: '#221430',
    color: '#9880a8',
    border: 'none',
    padding: '4px 10px',
    fontSize: 11,
    cursor: 'pointer',
    borderRadius: '3px 3px 0 0',
    fontFamily: 'inherit',
  },
  tabActive: {
    background: '#1a1020',
    color: '#e0d0f0',
  },
  output: {
    flex: 1,
    overflow: 'auto',
    padding: '4px 8px',
    fontFamily: 'monospace',
    fontSize: 11,
    lineHeight: 1.5,
  },
  line: {
    color: '#c0b0d0',
    whiteSpace: 'pre-wrap',
    wordBreak: 'break-all',
  },
  lineError: {
    color: '#f06060',
  },
  lineWarn: {
    color: '#f0c060',
  },
  codeArea: {
    flex: 1,
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
  },
}
