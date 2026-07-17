export function MenuBar() {
  return (
    <div style={styles.bar}>
      <div style={styles.menuGroup}>
        <span style={styles.menuItem} onClick={() => {}}>File</span>
        <span style={styles.menuItem} onClick={() => {}}>Edit</span>
        <span style={styles.menuItem} onClick={() => {}}>Run</span>
        <span style={styles.menuItem} onClick={() => {}}>View</span>
      </div>
    </div>
  )
}

const styles: Record<string, React.CSSProperties> = {
  bar: {
    height: 28,
    background: '#221430',
    display: 'flex',
    alignItems: 'center',
    padding: '0 8px',
    borderBottom: '1px solid #332044',
    userSelect: 'none',
  },
  menuGroup: {
    display: 'flex',
    gap: 2,
  },
  menuItem: {
    padding: '4px 10px',
    color: '#e0d0f0',
    fontSize: 12,
    cursor: 'pointer',
    borderRadius: 3,
  },
}
