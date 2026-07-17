import { Icon } from '@/components/Icon'

interface Symbol {
  name: string
  type: 'function' | 'class' | 'variable'
  line: number
}

interface OutlineProps {
  symbols: Symbol[]
  onNavigate: (line: number) => void
}

const typeIcons: Record<string, string> = {
  function: 'Script',
  class: 'Node',
  variable: 'File',
}

export function Outline({ symbols, onNavigate }: OutlineProps) {
  if (symbols.length === 0) {
    return <div style={styles.empty}>No symbols found</div>
  }

  return (
    <div style={styles.container}>
      {symbols.map((sym, i) => (
        <div
          key={`${sym.name}-${i}`}
          onClick={() => onNavigate(sym.line)}
          style={styles.item}
        >
          <Icon name={typeIcons[sym.type] ?? 'File'} size={12} />
          <span style={styles.name}>{sym.name}</span>
          <span style={styles.type}>{sym.type}</span>
          <span style={styles.line}>:{sym.line}</span>
        </div>
      ))}
    </div>
  )
}

const styles: Record<string, React.CSSProperties> = {
  container: {
    padding: '2px 0',
    overflow: 'auto',
    flex: 1,
  },
  item: {
    display: 'flex',
    alignItems: 'center',
    gap: 6,
    padding: '2px 8px',
    cursor: 'pointer',
    fontSize: 11,
    color: '#e0d0f0',
  },
  name: {
    flex: 1,
    overflow: 'hidden',
    textOverflow: 'ellipsis',
    whiteSpace: 'nowrap',
  },
  type: {
    color: '#9880a8',
    fontSize: 9,
    textTransform: 'uppercase',
  },
  line: {
    color: '#5a4a6a',
    fontSize: 9,
  },
  empty: {
    padding: 20,
    color: '#443055',
    fontSize: 12,
    textAlign: 'center',
  },
}
