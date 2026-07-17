import { Icon } from '@/components/Icon'

interface Toolbar2DProps {
  tool: string
  onToolChange: (tool: string) => void
  snapEnabled: boolean
  onSnapToggle: () => void
  gridEnabled: boolean
  onGridToggle: () => void
}

const tools = [
  { id: 'select', icon: 'ToolSelect', title: 'Select (Q)' },
  { id: 'move', icon: 'ToolMove', title: 'Move (W)' },
  { id: 'rotate', icon: 'ToolRotate', title: 'Rotate (E)' },
  { id: 'scale', icon: 'ToolScale', title: 'Scale (R)' },
]

const viewBtns = [
  { id: 'zoom', icon: 'ZoomIn', title: 'Frame Selected' },
  { id: 'center', icon: 'PanView', title: 'Center View' },
  { id: 'search', icon: 'Search', title: 'Search / Focus' },
]

export function Toolbar2D({ tool, onToolChange, snapEnabled, onSnapToggle, gridEnabled, onGridToggle }: Toolbar2DProps) {
  return (
    <div style={styles.bar}>
      {tools.map(t => (
        <button
          key={t.id}
          onClick={() => onToolChange(t.id)}
          title={t.title}
          style={{
            ...styles.toolBtn,
            ...(tool === t.id ? styles.toolBtnActive : {}),
          }}
        >
          <Icon name={t.icon} size={16} />
        </button>
      ))}

      <div style={styles.separator} />

      <button onClick={onSnapToggle} title="Snap toggle" style={{ ...styles.toolBtn, ...(snapEnabled ? styles.toolBtnActive : {}) }}>
        <Icon name="SnapGrid" size={16} />
      </button>
      <button onClick={onGridToggle} title="Grid view" style={{ ...styles.toolBtn, ...(gridEnabled ? styles.toolBtnActive : {}) }}>
        <Icon name="GridView" size={16} />
      </button>

      <div style={styles.separator} />

      {viewBtns.map(b => (
        <button key={b.id} title={b.title} style={styles.toolBtn}>
          <Icon name={b.icon} size={16} />
        </button>
      ))}
    </div>
  )
}

const styles: Record<string, React.CSSProperties> = {
  bar: {
    height: 30,
    background: '#2a1838',
    display: 'flex',
    alignItems: 'center',
    padding: '0 6px',
    gap: 1,
    borderBottom: '1px solid #332044',
  },
  toolBtn: {
    background: '#221430',
    border: 'none',
    width: 26,
    height: 26,
    cursor: 'pointer',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    borderRadius: 2,
    padding: 4,
  },
  toolBtnActive: {
    background: '#1a1020',
    outline: '1px solid #8b4fc8',
  },
  separator: {
    width: 1,
    height: 18,
    background: '#443055',
    margin: '0 6px',
  },
}
