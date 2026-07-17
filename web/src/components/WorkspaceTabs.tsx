import { Icon } from '@/components/Icon'
import { useGame } from '@/contexts/GameContext'
import { useConsole } from '@/contexts/ConsoleContext'

interface WorkspaceTabsProps {
  activeTab: string
  onTabChange: (tab: string) => void
}

export function WorkspaceTabs({ activeTab, onTabChange }: WorkspaceTabsProps) {
  const { isRunning, start, stop, mode, toggleMode } = useGame()
  const { append, clear } = useConsole()

  const tabs = [
    { id: '2d', label: '2D' },
    { id: 'script', label: 'SCRIPT' },
    { id: 'sprite', label: 'SPRITES' },
  ]

  const handlePlay = () => {
    clear()
    append(`--- Starting ${mode} mode ---`, 'info')
    start()
  }

  const handleStop = () => {
    stop()
    append('--- Stopped ---', 'info')
  }

  return (
    <div style={styles.bar}>
      <div style={styles.tabs}>
        {tabs.map(tab => (
          <button
            key={tab.id}
            onClick={() => onTabChange(tab.id)}
            style={{
              ...styles.tab,
              ...(activeTab === tab.id ? styles.tabActive : {}),
            }}
          >
            {tab.label}
          </button>
        ))}
        <button
          onClick={toggleMode}
          style={{
            ...styles.tab,
            ...(activeTab === mode ? styles.tabActive : {}),
          }}
        >
          {mode === 'game' ? 'GAME' : 'CONSOLE'}
        </button>
      </div>

      <div style={styles.controls}>
        {!isRunning ? (
          <button onClick={handlePlay} style={styles.playBtn} title="Run (F5)">
            <Icon name="Play" size={14} />
          </button>
        ) : (
          <button onClick={handleStop} style={styles.stopBtn} title="Stop (Shift+F5)">
            <Icon name="Stop" size={14} />
          </button>
        )}
      </div>
    </div>
  )
}

const styles: Record<string, React.CSSProperties> = {
  bar: {
    height: 28,
    background: '#2a1838',
    display: 'flex',
    alignItems: 'center',
    padding: '0 4px',
    borderBottom: '1px solid #332044',
  },
  tabs: {
    display: 'flex',
    flex: 1,
    gap: 0,
  },
  tab: {
    background: '#2a1838',
    color: '#9880a8',
    border: 'none',
    padding: '6px 14px',
    fontSize: 11,
    fontWeight: 700,
    cursor: 'pointer',
    borderRadius: 0,
    fontFamily: 'inherit',
  },
  tabActive: {
    background: '#1a1020',
    color: '#e0d0f0',
  },
  controls: {
    display: 'flex',
    gap: 4,
    padding: '0 4px',
  },
  playBtn: {
    background: '#5a2d8a',
    border: 'none',
    width: 28,
    height: 22,
    cursor: 'pointer',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    borderRadius: 2,
  },
  stopBtn: {
    background: '#8b3a62',
    border: 'none',
    width: 28,
    height: 22,
    cursor: 'pointer',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    borderRadius: 2,
  },
}
